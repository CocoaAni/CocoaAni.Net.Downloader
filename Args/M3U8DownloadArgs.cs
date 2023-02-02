using CocoaAni.Files.M3U8;

namespace CocoaAni.Net.Downloader.Args;

public class M3U8DownloadArgs : HttpDownloadArgs
{
    public M3U8DownloadArgs(string? url = null, string? savePath = null) : base(url, savePath)
    {
    }

    public M3U8DownloadArgs()
    {
    }

    public M3U8DownloadArgs(string? name, string? url, string? savePath) : base(name, url, savePath)
    {
    }

    public Action<MediaM3U8>? M3U8Config { get; set; }
}