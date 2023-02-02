using CocoaAni.Net.Downloader.Exceptions;

namespace CocoaAni.Net.Downloader.Events;

public delegate void DownloadEventHandler<in THandle>(THandle handle);

public delegate void FileDownloadStartEventHandler<in THandle>(THandle handle);

public delegate void FileDownloadEndEventHandler<in THandle>(THandle handle);

public delegate void FileDownloadSuccessEventHandler<in THandle, in TResult>(THandle handle, TResult result);

public delegate void FileDownloadErrorEventHandler<in THandle>(THandle handle, DownloadException ex);

public delegate void FileDownloadProgressUpdateEventHandler<in THandle>(THandle handle, float progress);