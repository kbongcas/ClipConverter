using ClipConverter.Models;
using ClipConverter.Services;
using FFmpeg.NET;
namespace ClipConverter;

public class ClipConverterRunner
{
    public QueueService _queueService;
    public StorageService _storageService;
    public ClipConverterService _clipConverterService;

    public ClipConverterRunner(
        QueueService queueService,
        StorageService storageService,
        ClipConverterService clipConverterService )

    {
        _clipConverterService = clipConverterService;
        _queueService = queueService;
        _storageService = storageService;
    }

    public async Task Run()
    {
        var message = await _queueService.PeekMessageAsync();
        if (String.IsNullOrEmpty(message.Id)) return;

        var blob = await _storageService.GetFileAsync(message.Message);
        if (blob == null) return;

        MediaFile convertedFile;
        using(blob.Content)
        {
            convertedFile = await _clipConverterService.ConvertClipToGif(blob);
            if (convertedFile == null) return;
        }

        using (var stream = System.IO.File.OpenRead(convertedFile.FileInfo.FullName))
        {
            ConvertedClip convertedClip = new ConvertedClip { Id = message.Message, Data = stream };
            await _storageService.UploadAsync(convertedClip);
        }

        await _queueService.RemoveMessageAsync(message);
    }
}
