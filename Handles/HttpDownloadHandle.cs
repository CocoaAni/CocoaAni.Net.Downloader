using CocoaAni.Files.Http;
using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Events;

namespace CocoaAni.Net.Downloader.Handles;

public class HttpDownloadHandle : BaseHttpDownloadHandle<HttpDownloadArgs, HttpFile, HttpDownloadHandle>
{
    public HttpDownloadHandle()
    {
    }

    public HttpDownloadHandle(HttpDownloadArgs? args, IDownloadEvents<HttpDownloadHandle>? events) : base(args, events)
    {
    }
}