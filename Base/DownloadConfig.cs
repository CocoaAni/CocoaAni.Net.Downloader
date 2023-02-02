namespace CocoaAni.Net.Downloader.Base;

public class DownloadConfig
{
    public static string DefaultFileSavePath { get; set; } = string.Empty;
    public static string FileSavePathIsMemory { get; set; } = string.Empty;
    public static int MaxFileSaveMemoryCapacity { get; set; }
}