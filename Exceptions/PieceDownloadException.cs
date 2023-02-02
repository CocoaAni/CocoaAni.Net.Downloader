using CocoaAni.Net.Downloader.Base;
using System.Runtime.Serialization;

namespace CocoaAni.Net.Downloader.Exceptions;

public class PieceDownloadException : DownloadException
{
    public DownloadPiece Piece { get; set; }

    public PieceDownloadException(DownloadPiece piece)
    {
        Piece = piece;
    }

    protected PieceDownloadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Piece = null!;
    }

    public PieceDownloadException(string? message, DownloadPiece piece) : base(message)
    {
        Piece = piece;
    }

    public PieceDownloadException(string? message, Exception? innerException, DownloadPiece piece) : base(message, innerException)
    {
        Piece = piece;
    }
}