using CocoaAni.Net.Downloader.Handles;

namespace CocoaAni.Net.Downloader.Events;

public class DownloadEventArgs<THandle, TArg> : EventArgs
{
    public DownloadEventArgs(THandle handle, TArg arg)
    {
        Handle = handle;
        Arg = arg;
    }

    public THandle Handle { get; }
    public TArg Arg { get; }
}

public interface IDownloadEvents<THandle>
    where THandle : class, IDownloadHandle
{
    public event DownloadEventHandler<THandle>? OnStart;

    public event DownloadEventHandler<THandle>? OnEnd;

    public event DownloadEventHandler<THandle>? OnError;

    public event DownloadEventHandler<THandle>? OnSuccess;

    public event DownloadEventHandler<THandle>? OnProgressUpdate;

    public event DownloadEventHandler<THandle>? OnWarring;

    public void SendStartEvent(THandle handle);

    public void SendEndEvent(THandle handle);

    public void SendSuccessEvent(THandle handle);

    public void SendErrorEvent(THandle handle);

    public void SendWarringEvent(THandle handle);

    public void SendProgressUpdateEvent(THandle handle);
}