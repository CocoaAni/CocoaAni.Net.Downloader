using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Events;
using CocoaAni.Net.Downloader.Exceptions;
using CocoaAni.Net.Downloader.Handles;
using System.Diagnostics;

namespace CocoaAni.Net.Downloader.Base;

public abstract class BaseDownloader<THandle, TArgs, TResult, TThisHandle> :
    IDownloader<THandle, TArgs, TResult, TThisHandle>
    where THandle : BaseDownloadHandle<TArgs, TResult, TThisHandle>, new()
    where TArgs : BaseDownloadArgs, new()
    where TResult : class
    where TThisHandle : class, IDownloadHandle
{
    protected abstract Task<TResult> CreateResultAsync(THandle handle);

    protected abstract THandle CreateDownloadHandle(TArgs? args = null, IDownloadEvents<THandle>? events = null);

    /// <summary>
    /// 创建分片下载流 或者 多分片初始化
    /// 多分片初始化失败应转换为单分片或者抛异常
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="piece">分片</param>``
    /// <param name="ctk">取消令牌</param>
    /// <returns>下载流/null 代表初始化成功</returns>
    protected abstract Task<Stream?> CreatePieceDownloadStreamOrInitAsync(THandle handle, DownloadPiece piece, CancellationToken ctk = default);

    /// <summary>
    /// 创建下载任务
    /// </summary>
    /// <param name="handle">任务句柄</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务Task</returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual async Task CreateDownloadTaskAsync(THandle handle, CancellationToken cancellationToken = default)
    {
        try
        {
            //根据是否合并文件创建文件保存流
            if (handle.Args.IsMergeFile)
            {
                handle.FileSavedStream = CreateFileSaveStream(handle.FileSavedPath);
            }
            //发送下载开始事件
            handle.Events.SendStartEvent((handle as TThisHandle)!);

            switch (handle.IsResumeDownload)
            {
                //单个分片恢复下载
                case true when handle.Pieces!.Length == 1:
                    await DownloadPieceOrInitAsync(handle, handle.Pieces[0]!, cancellationToken);
                    await OnDownloadEndAsync(handle, cancellationToken);
                    return;

                //多个分片恢复下载
                case true when handle.Pieces!.Length > 1:
                    break;

                //正常的单分片或者多分片下载
                case false:
                    //创建第一个分片，单分片下载则也是最后一个分片
                    var firstDownloadPiece = new DownloadPiece(handle.DownloadUrl, handle.FileSavedPath, handle.FileSavedStream!);
                    //下面有三种情况
                    //1.等待单分片下载完成
                    //2.多分片下载初始化
                    //3.多分片下载初始化失败跳转到1
                    await DownloadPieceOrInitAsync(handle, firstDownloadPiece, cancellationToken);
                    //情况1或3  单分片下载结果处理
                    if (handle.PieceCount == 1)
                    {
                        await OnDownloadEndAsync(handle, cancellationToken);
                        return;
                    }
                    //情况2 后续处理
                    handle.Pieces = CreateDownloadPieces(handle, cancellationToken);
                    break;
            }
            //合并多分片多线程任务
            var mainTask = Task.Run(delegate
            {
                Task.WaitAll(CreatePiecesDownloadTasks(handle, handle.Pieces, cancellationToken));
            }, cancellationToken);
            await DownloadProgressEventUpdateTaskAsync(handle, mainTask, cancellationToken);
            await mainTask;
            if (handle.Args.IsMergeFile && !await MergeDownloadAsyncPieces(handle, cancellationToken))
            {
                throw new DownloadException("文件合并失败！");
            }
        }
        catch (Exception e)
        {
            handle.Error = e;
            handle.Events.SendErrorEvent((handle as TThisHandle)!);
        }

        await OnDownloadEndAsync(handle, cancellationToken);
    }

    /// <summary>
    /// 尝试合并下载好的分片
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="ctk">取消令牌</param>
    /// <returns>是否全部合并完成</returns>
    protected async Task<bool> MergeDownloadAsyncPieces(THandle handle, CancellationToken ctk)
    {
        var pieces = handle.Pieces!;
        foreach (var piece in pieces)
        {
            if (piece.IsMerged) continue;
            if (piece.Task != Task.CompletedTask)
                return false;
            await CopyStreamAsync(piece.FileSavedStream!, handle.FileSavedStream!, null, ctk);
            piece.IsMerged = true;
            //await OnDownloadPieceDestroyAsync(handle, piece, ctk);
        }
        return true;
    }

    /// <summary>
    /// 创建下载分片，多分片下载初始化成功后调用
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="ctk"></param>
    /// <returns>下载分片</returns>
    protected virtual DownloadPiece[] CreateDownloadPieces(THandle handle, CancellationToken ctk = default)
    {
        var pieces = new DownloadPiece[handle.PieceCount];
        var pieceRangStart = 0L;
        var onePieceHaveSize = handle.FileSize / handle.PieceCount;
        for (var pieceId = 0; pieceId < handle.PieceCount; pieceId++)
        {
            var pieceRangEnd = pieceRangStart + onePieceHaveSize;
            if (pieceRangEnd > handle.FileSize)
            {
                pieceRangEnd = handle.FileSize - 1;
            }
            var savePath = handle.FileSavedPath != "" ? $"{handle.PiecesCachePath}/{pieceId}.piece" : handle.FileSavedPath;
            pieces[pieceId] = new DownloadPiece(pieceId, 0L, pieceRangStart, pieceRangEnd, handle.DownloadUrl, savePath, CreateFileSaveStream(savePath));
            pieceRangStart = pieceRangEnd + 1;
            handle.PiecesQueue.Enqueue(pieces[pieceId]);
        }
        return pieces;
    }

    /// <summary>
    /// 批量创建分片下载任务
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="pieces">分片</param>
    /// <param name="ctk">取消令牌</param>
    /// <returns>任务Tasks</returns>
    protected virtual Task[] CreatePiecesDownloadTasks(THandle handle, IEnumerable<DownloadPiece> pieces, CancellationToken ctk = default)
    {
        var workers = new Task[handle.ThreadCount];
        var workQueue = new Queue<DownloadPiece>(pieces);
        for (var workerId = 0; workerId < handle.ThreadCount; workerId++)
        {
            var id = workerId;
            workers[workerId] = Task.Run(async delegate
            {
                while (true)
                {
                    DownloadPiece piece = null!;
                    lock (workQueue)
                    {
                        if (workQueue.Count <= 0)
                        {
                            break;
                        }
                        piece = workQueue.Dequeue();
                        piece.Task = workers[id];
                    }
                    await DownloadPieceOrInitAsync(handle, piece, ctk);
                    piece.Task = Task.CompletedTask;
                }
            }, ctk);
        }
        return workers;
    }

    /// <summary>
    /// 创建下载参数
    /// </summary>
    /// <param name="url">URL</param>
    /// <param name="savePath">保存路径</param>
    /// <returns>下载参数</returns>
    protected virtual TArgs CreateFileDownloadArgs(string url, string? savePath = null)
    {
        var args = new TArgs
        {
            DownloadUrl = url,
            FileSavedPath = savePath ?? string.Empty
        };
        return args;
    }

    /// <summary>
    /// 分片下载结束时调用
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="piece">分片</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task OnPieceDownloadEndAsync(THandle handle, DownloadPiece piece, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 下载结束时调用
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="ctk">取消令牌</param>
    /// <returns>任务Task</returns>
    protected virtual async Task OnDownloadEndAsync(THandle handle, CancellationToken ctk = default)
    {
        if (handle.Args.IsMergeFile && handle.FileSavedStream!.Length != handle.FileSize)
        {
            handle.Error = new Exception("文件下载完成，合并后大小不一致！");
            handle.Events.SendErrorEvent((handle as TThisHandle)!);
        }
        else
        {
            handle.Events.SendProgressUpdateEvent((handle as TThisHandle)!);
            handle.Result = await CreateResultAsync(handle);
            handle.Events.SendSuccessEvent((handle as TThisHandle)!);
        }
        handle.Events.SendEndEvent((handle as TThisHandle)!);
    }

    /// <summary>
    /// 下载分片或者下载初始化
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="piece">分片</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务Task</returns>
    protected virtual async Task DownloadPieceOrInitAsync(THandle handle, DownloadPiece piece, CancellationToken cancellationToken = default)
    {
        try
        {
            //如果是初始化的话Stream为空
            piece.DownloadStream = await CreatePieceDownloadStreamOrInitAsync(handle, piece, cancellationToken);
            //初始化 不下载
            if (piece.DownloadStream == null)
            {
                if (piece.DownloadUsingRes is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }

            var downloadTask = piece.CopyDownloadStreamToSaveStreamAsync(null, cancellationToken);
            //单分片下载 特殊处理
            if (handle.PieceCount == 1)
            {
                //启动单分片的进度更新任务
                var progressEventUpdateTask = DownloadProgressEventUpdateTaskAsync(handle, downloadTask, cancellationToken);
                await progressEventUpdateTask;
            }
            await downloadTask;
        }
        catch (Exception e)
        {
            var error = new PieceDownloadException("分片下载失败：" + e.Message, e, piece);
            Debug.WriteLine(error);
            handle.Error = error;
            piece.Error = error;
            handle.Events.SendErrorEvent((handle as TThisHandle)!);
        }

        await OnPieceDownloadEndAsync(handle, piece, cancellationToken);
    }

    /// <summary>
    /// 下载进度更新任务
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <param name="mainTask">主下载任务</param>
    /// <param name="ctk">取消令牌</param>
    /// <returns></returns>
    protected virtual async Task DownloadProgressEventUpdateTaskAsync(THandle handle, Task mainTask, CancellationToken ctk = default)
    {
        //单分片任务更新
        if (handle.PieceCount == 1)
        {
            while (!mainTask.IsCompleted)
            {
                handle.Events.SendProgressUpdateEvent((handle as TThisHandle)!);
                await Task.Delay(handle.Args.ProgressUpdateTimeInterval, ctk);
            }
            await mainTask;
            return;
        }
        //多分片任务更新
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var lastStopMs = stopWatch.ElapsedMilliseconds;
        while (!mainTask.IsCompleted)
        {
            //文件下载大小计算
            handle.FileSavedSize = handle.Pieces!.Sum((piece) => piece.SavedSize);
            //发送更新事件
            handle.Events.SendProgressUpdateEvent((handle as TThisHandle)!);
            //合并文件处理
            if (handle.Args.IsMergeFile)
            {
                await MergeDownloadAsyncPieces(handle, ctk);
            }
            //延时计算
            var stopMs = stopWatch.ElapsedMilliseconds;
            var usedMs = stopMs - lastStopMs;
            lastStopMs = stopMs;
            if (usedMs > handle.Args.ProgressUpdateTimeInterval)
                continue;
            var delayMs = handle.Args.ProgressUpdateTimeInterval - usedMs;
            if (delayMs <= 0) continue;
            await Task.Delay((int)delayMs, ctk);
            lastStopMs = stopWatch.ElapsedMilliseconds;
        }
        await mainTask;
    }

    //发送进度更新事件

    /// <summary>
    /// 创建文件保存流
    /// </summary>
    /// <param name="savePath">保存路径</param>
    /// <param name="isResume">是否是恢复下载</param>
    /// <returns>保存流</returns>
    /// <exception cref="ArgumentException"></exception>
    protected virtual Stream CreateFileSaveStream(string savePath, bool isResume = false)
    {
        if (savePath == string.Empty)
        {
            savePath = DownloadConfig.DefaultFileSavePath;
        }
        if (savePath == DownloadConfig.FileSavePathIsMemory)
        {
            return DownloadConfig.MaxFileSaveMemoryCapacity == 0
                ? new MemoryStream()
                : new MemoryStream(DownloadConfig.MaxFileSaveMemoryCapacity);
        }
        if (File.Exists(savePath) && !isResume)
        {
            File.Delete(savePath);
        }
        var dirEndIndex = savePath.LastIndexOf("/", StringComparison.Ordinal);
        if (dirEndIndex == -1)
        {
            dirEndIndex = savePath.LastIndexOf("\\", StringComparison.Ordinal);
        }
        if (dirEndIndex == -1)
        {
            throw new DownloadException("文件夹格式错误：" + savePath);
        }

        var dirPath = savePath[..dirEndIndex];
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        return File.Open(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
    }

    /// <summary>
    /// 拷贝流
    /// </summary>
    /// <param name="source">源</param>
    /// <param name="target">目标</param>
    /// <param name="bufferSize">缓冲区大小</param>
    /// <param name="ctk">取消令牌</param>
    /// <returns></returns>
    protected virtual async Task CopyStreamAsync(Stream source, Stream target, int? bufferSize = null, CancellationToken ctk = default)
    {
        if (source.CanSeek)
        {
            source.Seek(0L, SeekOrigin.Begin);
        }
        var buffer = new byte[bufferSize ?? 1048576];
        while (true)
        {
            var readCount = await source.ReadAsync(buffer, ctk);
            if (readCount == 0) break;
            await target.WriteAsync(buffer.AsMemory(0, readCount), ctk);
            await target.FlushAsync(ctk);
        }
    }

    public Task<THandle> DownloadAsync(TArgs args, IDownloadEvents<THandle> events, CancellationToken cancellationToken = default)
    {
        return DownloadAsync(CreateDownloadHandle(args, events), cancellationToken);
    }

    public Task<THandle> DownloadAsync(TArgs args, CancellationToken cancellationToken = default)
    {
        return DownloadAsync(CreateDownloadHandle(args), cancellationToken);
    }

    public Task<THandle> DownloadAsync(string url, string savePath, CancellationToken cancellationToken = default)
    {
        return DownloadAsync(CreateDownloadHandle(CreateFileDownloadArgs(url, savePath)), cancellationToken);
    }

    public Task<THandle> DownloadAsync(string url, CancellationToken cancellationToken = default)
    {
        return DownloadAsync(CreateDownloadHandle(CreateFileDownloadArgs(url)), cancellationToken);
    }

    public Task<THandle> DownloadAsync(Func<THandle> handleCreator, CancellationToken cancellationToken = default)
    {
        return DownloadAsync(handleCreator(), cancellationToken);
    }

    public Task<THandle> DownloadAsync(Action<THandle> handleInitializer, CancellationToken cancellationToken = default)
    {
        var handle = CreateDownloadHandle();
        handleInitializer(handle);
        return DownloadAsync(handle, cancellationToken);
    }

    public async Task<THandle> DownloadAsync(THandle handle, CancellationToken cancellationToken = default)
    {
        var task = handle.Task = CreateDownloadTaskAsync(handle, cancellationToken);
        await task;
        return handle;
    }

    public THandle AsyncDownload(TArgs args, IDownloadEvents<THandle> events, CancellationToken cancellationToken = default)
    {
        return AsyncDownload(CreateDownloadHandle(args, events), cancellationToken);
    }

    public THandle AsyncDownload(TArgs args, CancellationToken cancellationToken = default)
    {
        return AsyncDownload(CreateDownloadHandle(args), cancellationToken);
    }

    public THandle AsyncDownload(string url, string savePath, CancellationToken cancellationToken = default)
    {
        return AsyncDownload(CreateDownloadHandle(CreateFileDownloadArgs(url, savePath)), cancellationToken);
    }

    public THandle AsyncDownload(string url, CancellationToken cancellationToken = default)
    {
        return AsyncDownload(CreateDownloadHandle(CreateFileDownloadArgs(url)), cancellationToken);
    }

    public THandle AsyncDownload(Action<THandle> handleInitializer, CancellationToken cancellationToken = default)
    {
        var handle = CreateDownloadHandle();
        handleInitializer(handle);
        return AsyncDownload(handle, cancellationToken);
    }

    public THandle AsyncDownload(Func<THandle> handleCreator, CancellationToken cancellationToken = default)
    {
        return AsyncDownload(handleCreator(), cancellationToken);
    }

    public THandle AsyncDownload(THandle handle, CancellationToken cancellationToken = default)
    {
        handle.Task = CreateDownloadTaskAsync(handle, cancellationToken);
        return handle;
    }
}