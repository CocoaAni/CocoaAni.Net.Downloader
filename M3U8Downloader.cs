using CocoaAni.Files.M3U8;
using CocoaAni.Net.Downloader.Args;
using CocoaAni.Net.Downloader.Base;
using CocoaAni.Net.Downloader.Events;
using CocoaAni.Net.Downloader.Handles;

namespace CocoaAni.Net.Downloader;

public class M3U8Downloader : BaseHttpDownloader<M3U8DownloadHandle, M3U8DownloadArgs, TransportStreamFiles, M3U8DownloadHandle>
{
    protected override Task<TransportStreamFiles> CreateResultAsync(M3U8DownloadHandle handle)
    {
        var result = new TransportStreamFiles(handle.FileSavedPath);
        foreach (var downloadPiece in handle.Pieces!)
        {
            result.Paths.Add(downloadPiece.SavePath);
        }
        return Task.FromResult(result);
    }

    protected override M3U8DownloadHandle CreateDownloadHandle(M3U8DownloadArgs? args = null,
        IDownloadEvents<M3U8DownloadHandle>? events = null)
    => new(args, events);

    protected override async Task<Stream?> CreatePieceDownloadStreamOrInitAsync(M3U8DownloadHandle handle, DownloadPiece piece,
        CancellationToken ctk = default)
    {
        var stream = await base.CreatePieceDownloadStreamOrInitAsync(handle, piece, ctk);
        if (handle.MediaM3U8 != null)
        {
            return stream;
        }
        var reader = new StreamReader(piece.DownloadStream!);
        handle.MediaM3U8 = MediaM3U8.Parse(await reader.ReadToEndAsync());
        handle.Args.M3U8Config?.Invoke(handle.MediaM3U8);
        handle.PieceCount = handle.MediaM3U8.PlayLists.Count;
        reader.BaseStream.Close();
        reader.Close();
        return null;
    }

    protected override DownloadPiece[] CreateDownloadPieces(M3U8DownloadHandle handle, CancellationToken ctk = default)
    {
        var m3U8 = handle.MediaM3U8!;
        var pieces = new DownloadPiece[m3U8.PlayLists.Count];
        for (var pieceId = 0; pieceId < pieces.Length; pieceId++)
        {
            var playListItem = m3U8.PlayLists[pieceId];
            var savePath = ((handle.FileSavedPath != "") ? $"{handle.PiecesCachePath}/{pieceId}.ts" : handle.FileSavedPath);
            pieces[pieceId] = new DownloadPiece(pieceId, 0L, 0, -1, playListItem.Title!, savePath, CreateFileSaveStream(savePath));
            handle.PiecesQueue.Enqueue(pieces[pieceId]);
        }
        return pieces;
    }

    protected override Task DownloadProgressEventUpdateTaskAsync(M3U8DownloadHandle handle, Task mainTask,
        CancellationToken ctk = default)
    {
        return Task.CompletedTask;
    }

    protected override Task OnPieceDownloadEndAsync(M3U8DownloadHandle handle, DownloadPiece piece,
        CancellationToken ctk = default)
    {
        handle.Events.SendProgressUpdateEvent(handle);
        piece.FileSavedStream?.Close();
        return base.OnPieceDownloadEndAsync(handle, piece, ctk);
    }
}