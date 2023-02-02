using CocoaAni.Net.Downloader.Args;

namespace CocoaAni.Net.Downloader.Base;

public interface IDownloadTaskInfo : IDownloadArgs
{
    public long FileSize { get; }
    public long FileSavedSize { get; }
    public DownloadPiece[]? Pieces { get; }
}

/*
public class DownloadTaskInfo : IDownloadTaskInfo
{
    public DownloadTaskInfo(string taskName, DownloadProtocol protocol, string savePath, string downloadUrl, long fileSize, long savedSize, int progressEventUpdateTimeInterval, int timeout, int maxPieceCount, DownloadPiece[] taskPieces, float speedCalculateSecondInterval, int maxPieceUseTaskCount, bool isResumeDownload)
    {
        TaskName = taskName;
        Protocol = protocol;
        FileSavedPath = savePath;
        DownloadUrl = downloadUrl;
        FileSize = fileSize;
        FileSavedSize = savedSize;
        ProgressUpdateTimeInterval = progressEventUpdateTimeInterval;
        Timeout = timeout;
        PieceCount = maxPieceCount;
        Pieces = taskPieces;
        SpeedCalculateInterval = speedCalculateSecondInterval;
        ThreadCount = maxPieceUseTaskCount;
        IsResumeDownload = isResumeDownload;
    }

    public string TaskName { get; set; }
    public DownloadProtocol Protocol { get; }
    public string FileSavedPath { get; set; }
    public string DownloadUrl { get; set; }
    public long FileSize { get; set; }
    public long FileSavedSize { get; set; }
    public int ProgressUpdateTimeInterval { get; }
    public int SpeedCalculateTimeInterval { get; }
    public float SpeedCalculateInterval { get; }
    public int Timeout { get; set; }
    public int PieceCount { get; set; }
    public int ThreadCount { get; }
    public bool IsResumeDownload { get; }
    public DownloadPiece[]? Pieces { get; set; }
}*/