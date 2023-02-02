using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Enums;
using CocoaAni.Net.Downloader.Handles;
using System.Net;
using System.Net.Http.Headers;

namespace CocoaAni.Net.Downloader.Base;
#pragma warning disable CA1822
#pragma warning disable SYSLIB0014
/*
public abstract class BaseHttpDownloader<THandle, TArgs, TResult> : BaseFileDownloader<THandle, TArgs, TResult>,
    IFileDownloader<THandle, TArgs, TResult>
    where TArgs : HttpDownloadArgs, new()
    where TResult : HttpFile
    where THandle : HttpFileDownloadHandle, IFileDownloadHandle<TArgs, TResult, THandle>, new()
 */

public abstract class BaseHttpDownloader<THandle, TArgs, TResult, TThisHandle> :
    BaseDownloader<THandle, TArgs, TResult, TThisHandle>,
    IDownloader<THandle, TArgs, TResult, TThisHandle>
    where TResult : class
    where TArgs : HttpDownloadArgs, new()
    where TThisHandle : class, IDownloadHandle//BaseDownloadHandle<TArgs, TResult, TThisHandle>
    where THandle : BaseHttpDownloadHandle<TArgs, TResult, TThisHandle>, new()
{
    protected override async Task<Stream?> CreatePieceDownloadStreamOrInitAsync(THandle handle, DownloadPiece piece,
        CancellationToken ctk = default)
    {
        if (handle.Args.EnableUseHttpClient)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage? resp = null;
            if (handle.Args.PieceCount > 1 && piece.RangeEnd == 0)
            {
                resp = await httpClient.GetAsync(piece.DownloadUrl, ctk);
                handle.FileSize = resp.Content.Headers.ContentLength ?? -1;
                if (handle.FileSize != -1)
                {
                    //文件可以多分片下载 窥探结束
                    handle.PieceCount = handle.Args.PieceCount;
                    return default!;
                }
                //文件不可以多分片下载 转换为普通下载
                handle.Warring = new DownloaderWarring(DownloadWarringCode.NotSupportMultiplePriceDownload,
                    "当前文件不支持多分片下载！已转换为普通下载。", null);
                handle.Events.SendWarringEvent((handle as TThisHandle)!);
                handle.PieceCount = 1;
            }
            else if (piece.RangeEnd != 0)
            {
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(piece.RangeStart, piece.RangeEnd);
            }
            resp ??= await httpClient.GetAsync(piece.DownloadUrl, ctk);

            var stream = await resp.Content.ReadAsStreamAsync(ctk);
            if (handle.FileSize == 0)
                handle.FileSize = resp.Content.Headers.ContentLength ?? -1;
            return stream;
        }
        else
        {
            var request = (HttpWebRequest)WebRequest.Create(piece.DownloadUrl);
            request.Timeout = handle.Args.Timeout;
            request.ReadWriteTimeout = handle.Args.Timeout;
            WebResponse? resp = null;
            //初始化多切片下载
            if (handle is { PieceCount: > 1, Pieces: null })
            {
                piece.DownloadUsingRes = resp = request.GetResponse();
                handle.FileSize = resp.ContentLength;
                //文件可以多分片下载 窥探结束
                if (handle.FileSize != -1)
                {
                    //文件可以多分片下载 窥探结束
                    handle.PieceCount = handle.Args.PieceCount;
                    handle.ThreadCount = handle.Args.ThreadCount;
                    piece.DownloadStream = resp.GetResponseStream();
                    return default;
                }
                //文件不可以多分片下载

                handle.Warring = new DownloaderWarring(DownloadWarringCode.NotSupportMultiplePriceDownload,
                    "当前文件不支持多分片下载！已转换为普通下载。", null);
                handle.Events.SendWarringEvent((handle as TThisHandle)!);
                handle.PieceCount = 1;
                handle.ThreadCount = 1;
            }
            //范围下载设置
            else if (piece.RangeEnd > 0)
            {
                request.AddRange(piece.RangeStart, piece.RangeEnd);
            }
            resp ??= await request.GetResponseAsync();
            if (handle.FileSize == 0)
                handle.FileSize = resp.ContentLength;

            return piece.DownloadStream = resp.GetResponseStream();
        }
    }
}