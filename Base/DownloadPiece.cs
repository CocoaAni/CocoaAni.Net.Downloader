namespace CocoaAni.Net.Downloader.Base;

public class DownloadPiece
{
    public DownloadPiece(string downloadUrl, string savePath, Stream fileSavedStream)
    {
        DownloadUrl = downloadUrl;
        SavePath = savePath;
        FileSavedStream = fileSavedStream;
    }

    public DownloadPiece(int id, long savedSize, long rangeStart, long rangeEnd, string downloadUrl, string savePath, Stream fileSavedStream)
    {
        SavedSize = savedSize;
        Id = id;
        RangeStart = rangeStart;
        RangeEnd = rangeEnd;
        DownloadUrl = downloadUrl;
        SavePath = savePath;
        FileSavedStream = fileSavedStream;
    }

    public int Id { get; set; }
    public long RangeStart { get; set; }
    public long RangeEnd { get; set; }
    public long RangeLength => RangeEnd - RangeStart + 1;

    public long SavedSize { get; set; }
    public string DownloadUrl { get; set; }
    public string SavePath { get; set; }
    public Exception? Error { get; set; }
    public Stream? FileSavedStream { get; set; }
    public Stream? DownloadStream { get; set; }
    public object? DownloadUsingRes { get; set; }

    public Task? Task { get; set; }
    public bool IsMerged { get; set; }

    public bool IsError => Error == null;
    public bool IsSuccess => Error != null;

    public async Task CopyDownloadStreamToSaveStreamAsync(int? bufferSize = null, CancellationToken cancellationToken = default)
    {
        if (DownloadStream == null) throw new NullReferenceException("下载流为空!");
        if (DownloadStream.CanSeek)
            DownloadStream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[bufferSize ?? 1048576];
        int readCount;
        while ((readCount = await DownloadStream.ReadAsync(buffer, cancellationToken)) != 0)
        {
            await FileSavedStream!.WriteAsync(buffer.AsMemory(0, readCount), cancellationToken);
            await FileSavedStream.FlushAsync(cancellationToken);
            SavedSize += readCount;
        }
    }
}