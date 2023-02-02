using CocoaAni.Files.Http;
using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Base;
using CocoaAni.Net.Downloader.Events;
using CocoaAni.Net.Downloader.Handles;

namespace CocoaAni.Net.Downloader;

public class HttpDownloader : BaseHttpDownloader<HttpDownloadHandle, HttpDownloadArgs, HttpFile, HttpDownloadHandle>
{
    protected override HttpDownloadHandle CreateDownloadHandle(HttpDownloadArgs? args = null,
        IDownloadEvents<HttpDownloadHandle>? events = null) => new(args, events);

    protected override Task<HttpFile> CreateResultAsync(HttpDownloadHandle handle)
    {
        return Task.FromResult(new HttpFile(handle.FileSavedPath, handle.DownloadUrl));
    }

    public static HttpDownloader Instance { get; } = new HttpDownloader();
}