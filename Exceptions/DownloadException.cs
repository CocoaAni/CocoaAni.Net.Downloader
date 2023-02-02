using System.Runtime.Serialization;

namespace CocoaAni.Net.Downloader.Exceptions;

public class DownloadException : Exception
{
    public DownloadException()
    {
    }

    protected DownloadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public DownloadException(string? message) : base(message)
    {
    }

    public DownloadException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}