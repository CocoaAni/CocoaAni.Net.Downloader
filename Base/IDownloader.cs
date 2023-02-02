using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Events;
using CocoaAni.Net.Downloader.Handles;

namespace CocoaAni.Net.Downloader.Base;

public interface IDownloader
{
}

public interface IDownloader<THandle, in TArgs, TResult, TThisHandle>
    where TArgs : class, IDownloadArgs
    where TResult : class
    where THandle : class, IDownloadHandle<TArgs, TResult, TThisHandle>, IDownloadHandle
    where TThisHandle : class, IDownloadHandle
{
    Task<THandle> DownloadAsync(TArgs args, IDownloadEvents<THandle> events, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(TArgs args, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(string url, string savePath, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(string url, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(Func<THandle> handleCreator, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(Action<THandle> handleInitializer, CancellationToken cancellationToken = default);

    Task<THandle> DownloadAsync(THandle handle, CancellationToken cancellationToken = default);

    THandle AsyncDownload(TArgs args, IDownloadEvents<THandle> events, CancellationToken cancellationToken = default);

    THandle AsyncDownload(TArgs args, CancellationToken cancellationToken = default);

    THandle AsyncDownload(string url, string savePath, CancellationToken cancellationToken = default);

    THandle AsyncDownload(string url, CancellationToken cancellationToken = default);

    THandle AsyncDownload(Action<THandle> handleInitializer, CancellationToken cancellationToken = default);

    THandle AsyncDownload(Func<THandle> handleCreator, CancellationToken cancellationToken = default);

    THandle AsyncDownload(THandle handle, CancellationToken cancellationToken = default);
}