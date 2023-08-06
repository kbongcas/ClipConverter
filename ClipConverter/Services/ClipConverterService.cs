using ClipConverter.Models;
using FFmpeg.NET;
using Microsoft.Extensions.Configuration;

namespace ClipConverter.Services;
public class ClipConverterService : IClipConverterService
{
    private IConfiguration _config;

    public ClipConverterService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<MediaFile> ConvertClipToGif(Blob blob)
    {
        var ffmpegPath = _config.GetValue<string>("FfmpegExecPath");
        var ffmpeg = new Engine(ffmpegPath);

        StreamInput streamInput = new StreamInput(blob.Content);
        var outputFilePath = _config.GetValue<string>("ConvertedClipOutputDir") + blob.Name + ".gif";
        OutputFile fileOutput = new OutputFile(outputFilePath);

        ConversionOptions conversionOptions = new ConversionOptions
        {
            VideoFps = 30
        };
        var token = new CancellationToken();

        var file = await ffmpeg.ConvertAsync(streamInput, fileOutput, conversionOptions, token);

        return file;
    }
}
