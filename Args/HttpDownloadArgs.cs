namespace CocoaAni.Net.Downloader.Args;

public class HttpDownloadArgs : BaseDownloadArgs
{
    public HttpDownloadArgs(string? url = null, string? savePath = null) : base(url, savePath)
    {
    }

    public HttpDownloadArgs()
    {
    }

    public HttpDownloadArgs(string? name, string? url, string? savePath) : base(name, url, savePath)
    {
    }

    /// <summary>
    /// 不推荐启用，因为每个HttpClient第一次请求都需要预热，多分片下载非常慢
    /// </summary>
    public bool EnableUseHttpClient { get; set; } = false;
}