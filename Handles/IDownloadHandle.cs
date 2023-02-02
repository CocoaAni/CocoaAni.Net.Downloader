using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Base;
using CocoaAni.Net.Downloader.Enums;
using CocoaAni.Net.Downloader.Events;

namespace CocoaAni.Net.Downloader.Handles;

public interface IDownloadHandle
{
}

public interface IDownloadHandle<out TArgs, out TResult, TThis> : IDownloadHandle,
    IDownloadEvents<TThis>
    where TResult : class
    where TArgs : class, IDownloadArgs
    where TThis : class, IDownloadHandle
{
    public TArgs Args { get; }
    public Task? Task { get; }
    public bool IsSavePiece { get; set; }
    public int PieceCount { get; }
    public int ThreadCount { get; }
    public int PieceCompletedCount { get; }
    public Queue<DownloadPiece> PiecesQueue { get; }
    public List<Task> PieceDownloadUsedTasks { get; }

    public float Progress { get; }
    public long SpeedOfByte { get; }
    public double SpeedOfKB { get; }
    public double SpeedOfMB { get; }
    public double SpeedOfGB { get; }

    public Exception? Error { get; }
    public DownloaderWarring? Warring { get; }
    public DownloadState State { get; }
    public Stream? FileSavedStream { get; }
    public string PiecesCachePath { get; }
    public TResult? Result { get; }

    public IDownloadEvents<TThis> Events { get; }

    public Task<TThis> WaitDownloadedAsync();

    public void CalcDownloadSpeed();

    public void DeletePieceCaches();
}