using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Base;
using CocoaAni.Net.Downloader.Enums;
using CocoaAni.Net.Downloader.Events;

namespace CocoaAni.Net.Downloader.Handles;

public abstract class BaseDownloadHandle<TArgs, TResult, TThis> :
    IDownloadHandle<TArgs, TResult, TThis>,
    IDownloadEvents<TThis>
    where TResult : class
    where TArgs : BaseDownloadArgs, IDownloadArgs, new()
    where TThis : class, IDownloadHandle
{
    public IDownloadEvents<TThis> Events { get; protected internal set; }

    protected BaseDownloadHandle() : this(new(), null)
    {
        Events = this;
    }

    protected BaseDownloadHandle(TArgs? args, IDownloadEvents<TThis>? events)
    {
        Args = args ?? new TArgs();
        Events = events ?? this;
        PieceCount = Args.PieceCount;
        ThreadCount = Args.ThreadCount;
    }

    #region 公共属性

    public Task? Task { get; protected internal set; }
    public bool IsSavePiece { get; set; }
    public DownloadPiece[]? Pieces { get; protected internal set; }
    public Queue<DownloadPiece> PiecesQueue { get; } = new Queue<DownloadPiece>();
    public List<Task> PieceDownloadUsedTasks { get; } = new List<Task>();
    public int PieceCount { get; protected internal set; }
    public int ThreadCount { get; protected internal set; }
    public int PieceCompletedCount => Pieces == null ? 0 : Pieces.Length - PiecesQueue.Count;

    public int SpeedCalculateTimeInterval => Args.SpeedCalculateTimeInterval;
    public long SpeedOfByte { get; protected internal set; }
    public double SpeedOfKB => SpeedOfByte / 1024.0;
    public double SpeedOfMB => SpeedOfByte / 1048576.0;
    public double SpeedOfGB => SpeedOfByte / 1073741824.0;

    public bool IsResumeDownload => Pieces != null;

    public TArgs Args { get; protected internal set; }
    public string TaskName => Args.TaskName;

    public string FileSavedPath => Args.FileSavedPath;

    public Stream? FileSavedStream { get; protected internal set; }
    public virtual string PiecesCachePath => FileSavedPath + ".p-cache";
    public long FileSize { get; protected internal set; }
    public long FileSavedSize { get; protected internal set; }

    public string DownloadUrl => Args.DownloadUrl;

    public DownloadProtocol Protocol => DownloadProtocol.Http;
    public TResult? Result { get; protected internal set; }
    public Exception? Error { get; protected internal set; }
    public DownloadState State { get; protected internal set; }
    public DownloaderWarring? Warring { get; protected internal set; }

    public virtual float Progress
    {
        get
        {
            if (FileSavedSize == 0 || FileSize == -1)
            {
                return 0;
            }
            if (FileSavedStream!.Length == FileSize)
            {
                return 1f;
            }
            else
            {
                return FileSavedSize / (FileSize * 1.0f);
            }
        }
    }

    public int ProgressUpdateTimeInterval => Args.ProgressUpdateTimeInterval;

    #endregion 公共属性

    public async Task<TThis> WaitDownloadedAsync()
    {
        try
        {
            if (Task == null) throw new Exception("下载任务为空，请检查任务是否开始！");
            await Task;
            return (this as TThis)!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected CalculateSpeedHandle? CalcSpeedHandle;

    public void CalcDownloadSpeed()
    {
        CalcSpeedHandle ??= new CalculateSpeedHandle
        {
            CalcSecondInterval = SpeedCalculateTimeInterval / 1000f
        };
        var speedOfBytes = Utils.CalcSpeedOfBytes(CalcSpeedHandle, FileSavedSize);
        if (speedOfBytes != null) SpeedOfByte = speedOfBytes.Value;
    }

    public void DeletePieceCaches()
    {
        if (!Directory.Exists(PiecesCachePath)) return;
        if (Pieces != null)
        {
            foreach (var downloadPiece in Pieces)
            {
                downloadPiece.FileSavedStream?.Close();
            }
        }
        Directory.Delete(PiecesCachePath, true);
    }

    public event DownloadEventHandler<TThis>? OnStart;

    public event DownloadEventHandler<TThis>? OnEnd;

    public event DownloadEventHandler<TThis>? OnError;

    public event DownloadEventHandler<TThis>? OnSuccess;

    public event DownloadEventHandler<TThis>? OnProgressUpdate;

    public event DownloadEventHandler<TThis>? OnWarring;

    void IDownloadEvents<TThis>.SendStartEvent(TThis handle)
    {
        State = DownloadState.Start;
        if (Events != this)
        {
            Events?.SendStartEvent(handle);
            return;
        }
        OnStart?.Invoke((this as TThis)!);
    }

    void IDownloadEvents<TThis>.SendEndEvent(TThis handle)
    {
        State = DownloadState.End;
        if (Events != this)
        {
            Events?.SendEndEvent(handle);
            return;
        }

        OnEnd?.Invoke((this as TThis)!);
    }

    void IDownloadEvents<TThis>.SendSuccessEvent(TThis handle)
    {
        State = DownloadState.Success;
        if (Events != this)
        {
            Events?.SendSuccessEvent(handle);
            return;
        }
        OnSuccess?.Invoke((this as TThis)!);
    }

    void IDownloadEvents<TThis>.SendErrorEvent(TThis handle)
    {
        if (Events != this)
        {
            Events?.SendErrorEvent(handle);
            return;
        }
        OnError?.Invoke(handle);
    }

    void IDownloadEvents<TThis>.SendProgressUpdateEvent(TThis handle)
    {
        CalcDownloadSpeed();
        State = DownloadState.Downloading;
        if (Events != this)
        {
            Events?.SendProgressUpdateEvent(handle);
            return;
        }
        OnProgressUpdate?.Invoke(handle);
    }

    void IDownloadEvents<TThis>.SendWarringEvent(TThis handle)
    {
        if (Events != this)
        {
            Events?.SendWarringEvent(handle);
            return;
        }
        OnWarring?.Invoke(handle);
    }
}