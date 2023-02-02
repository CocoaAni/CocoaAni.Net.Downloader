using CocoaAni.Files.M3U8;
using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Events;

namespace CocoaAni.Net.Downloader.Handles;

public class M3U8DownloadHandle : BaseHttpDownloadHandle<M3U8DownloadArgs, TransportStreamFiles, M3U8DownloadHandle>
{
    public MediaM3U8? MediaM3U8 { get; set; }

    public new int PieceCount
    {
        get => base.PieceCount;
        internal set => base.PieceCount = value;
    }

    public M3U8DownloadHandle()
    {
    }

    internal int InternalPieceUsedTaskCount
    {
        set => ThreadCount = value;
    }

    public M3U8DownloadHandle(M3U8DownloadArgs? args, IDownloadEvents<M3U8DownloadHandle>? events) : base(args, events)
    {
    }

    public override float Progress
    {
        get
        {
            if (Pieces == null)
                return 0;
            var completedCount = Pieces.Count(t => t.Task == Task.CompletedTask);
            if (completedCount == 0)
                return 0;
            return (float)completedCount / Pieces.Length;
        }
    }
}