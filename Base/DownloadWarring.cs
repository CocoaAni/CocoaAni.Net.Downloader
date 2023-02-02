using CocoaAni.Net.Downloader.Enums;

namespace CocoaAni.Net.Downloader.Base;

public record DownloaderWarring(DownloadWarringCode Code, string Message, Exception? Exception);