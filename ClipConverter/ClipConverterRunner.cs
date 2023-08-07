using ClipConverter.Models;
using ClipConverter.Services;
using FFmpeg.NET;
using Microsoft.Extensions.Configuration;

namespace ClipConverter;

public class ClipConverterRunner
{
    private QueueService _queueService;
    private StorageService _storageService;
    private ClipConverterService _clipConverterService;


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
        Console.WriteLine($"Retrieivng Message form queue.");
        var message = await _queueService.PeekMessageAsync();
        Console.WriteLine($"Message Found: {message.Message}");
        if (String.IsNullOrEmpty(message.Id)) return;

        Console.WriteLine($"Downlaoding File From Storage.");
        var clip = await _storageService.DownloadAsync(message.Message);
        if (clip == null) return;
        Console.WriteLine($"File Found and Downloaded: {clip}");

        Console.WriteLine($"Converting file to gif.");
        var convertedFile = await _clipConverterService.ConvertClipToGif(clip);
        if (convertedFile == null) return;
        Console.WriteLine($"File Converted: {convertedFile}");

        using (var stream = System.IO.File.OpenRead(convertedFile))
        {
            Console.WriteLine($"Uploading File to storage.");
            ConvertedClip convertedClip = new ConvertedClip { Id = Path.GetFileName(convertedFile), Data = stream };
            await _storageService.UploadAsync(convertedClip);
            Console.WriteLine($"File uploaded: {convertedFile}");
        }

        Console.WriteLine($"Removing Message from queue");
        await _queueService.RemoveMessageAsync(message);
        Console.WriteLine($"Message Removed: {message}");
    }
}
