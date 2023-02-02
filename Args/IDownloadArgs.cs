using CocoaAni.Net.Downloader.Enums;

namespace CocoaAni.Net.Downloader.Args;

public interface IDownloadArgs
{
    string TaskName { get; }
    public DownloadProtocol Protocol { get; }
    public string DownloadUrl { get; }
    public string FileSavedPath { get; }
    public int ProgressUpdateTimeInterval { get; }
    public int SpeedCalculateTimeInterval { get; }
    public int Timeout { get; }
    public int PieceCount { get; }
    public int ThreadCount { get; }
    public bool IsResumeDownload { get; }
}