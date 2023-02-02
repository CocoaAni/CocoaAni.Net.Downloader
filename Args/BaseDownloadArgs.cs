using CocoaAni.Net.Downloader.Enums;

namespace CocoaAni.Net.Downloader.Args;

public abstract class BaseDownloadArgs : IDownloadArgs
{
    protected BaseDownloadArgs(string? url = null, string? savePath = null) : this(string.Empty, url, savePath)
    {
    }

    protected BaseDownloadArgs() : this(null, null, null)
    {
    }

    protected BaseDownloadArgs(string? name, string? url, string? savePath)
    {
        TaskName = name ?? string.Empty;
        DownloadUrl = url ?? string.Empty;
        FileSavedPath = savePath ?? string.Empty;
    }

    public string TaskName { get; set; }
    public DownloadProtocol DownloadProtocol { get; set; }
    public string DownloadUrl { get; set; }
    public string FileSavedPath { get; set; }
    public DownloadProtocol Protocol { get; set; }
    public float CalculateSpeedSecondInterval { get; set; } = DefaultCalculateSpeedSecondInterval;
    public int ProgressUpdateTimeInterval { get; set; } = DefaultProgressEventUpdateTimeInterval;
    public int SpeedCalculateTimeInterval { get; set; }
    public int Timeout { get; set; } = DefaultTimeout;
    public int PieceCount { get; set; } = DefaultMaxPieceCount;
    public int ThreadCount { get; set; } = DefaultMaxPieceUseTaskCount;
    public bool IsSavePiece { get; set; }
    public bool IsResumeDownload { get; set; }
    public bool IsMergeFile { get; set; } = true;

    #region 参数默认值

    public static int DefaultMaxPieceUseTaskCount { get; set; } = 8;
    public static float DefaultCalculateSpeedSecondInterval { get; set; } = 0.75f;
    public static int DefaultProgressEventUpdateTimeInterval { get; set; } = 500;
    public static int DefaultTimeout { get; set; } = 10000;
    public static int DefaultMaxPieceCount { get; set; } = 8;

    #endregion 参数默认值
}