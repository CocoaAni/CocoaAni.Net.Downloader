using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Events;

namespace CocoaAni.Net.Downloader.Handles;

public abstract class BaseHttpDownloadHandle<TArgs, TResult, TThis> :
    BaseDownloadHandle<TArgs, TResult, TThis>, IDownloadHandle
    where TArgs : HttpDownloadArgs, new()
    where TResult : class
    where TThis : class, IDownloadHandle
{
    protected BaseHttpDownloadHandle()
    {
    }

    protected BaseHttpDownloadHandle(TArgs? args, IDownloadEvents<TThis>? events) : base(args, events)
    {
    }
}