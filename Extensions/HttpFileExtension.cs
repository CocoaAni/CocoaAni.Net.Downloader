using CocoaAni.Files.Http;
using CocoaAni.Net.Downloader.Handles;

namespace CocoaAni.Net.Downloader.Extensions;

public static class HttpFileExtension
{
    public static Task<HttpDownloadHandle>? AsyncFileFromNetAsync(this HttpFile httpFile, Action<HttpDownloadHandle>? initializer = null)
    {
        var handle = new HttpDownloadHandle
        {
            Args =
            {
                FileSavedPath = httpFile.LocalPath,
                DownloadUrl = httpFile.NetUrl
            }
        };
        initializer?.Invoke(handle);
        return HttpDownloader.Instance.DownloadAsync(handle);
    }

    public static HttpDownloadHandle? AsyncFileFromInternet(this HttpFile httpFile, Action<HttpDownloadHandle>? initializer = null)
    {
        var handle = new HttpDownloadHandle
        {
            Args =
            {
                FileSavedPath = httpFile.LocalPath,
                DownloadUrl = httpFile.NetUrl
            }
        };
        initializer?.Invoke(handle);
        return HttpDownloader.Instance.AsyncDownload(handle);
    }

    public static HttpDownloadHandle AsyncDownload(this HttpFile httpFile, Action<HttpDownloadHandle>? initializer = null)
    {
        var handle = new HttpDownloadHandle
        {
            Args =
            {
                FileSavedPath = httpFile.LocalPath,
                DownloadUrl = httpFile.NetUrl
            }
        };
        initializer?.Invoke(handle);

        return HttpDownloader.Instance.AsyncDownload(handle);
    }

    public static Task<HttpDownloadHandle> DownloadAsync(this HttpFile httpFile, Action<HttpDownloadHandle>? initializer = null)
    {
        var handle = new HttpDownloadHandle
        {
            Args =
            {
                FileSavedPath = httpFile.LocalPath+"1",
                DownloadUrl = httpFile.NetUrl
            }
        };
        initializer?.Invoke(handle);
        return HttpDownloader.Instance.DownloadAsync(handle);
    }
}