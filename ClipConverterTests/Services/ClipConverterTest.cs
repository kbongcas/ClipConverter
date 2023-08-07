using ClipConverter.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;
using ClipConverter.Models;
using Blob = ClipConverter.Models.Blob;
using FFmpeg.NET;

namespace ClipConverterTests.Services;
public class ClipConverterTest
{
    private ClipConverterService _clipConverterService;
    private readonly string ffmpegPath = "C:\\ProgramData\\chocolatey\\bin\\ffmpeg.exe";
    private readonly string _clipOutputDirPath = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/clips/";
    private readonly string _convertedOutputDir = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/converted/";

    [SetUp]
    public void SetUp()
    {
        var inMemConfig = new Dictionary<string, string> {
            {"FfmpegExecPath", ffmpegPath},
            {"ClipOutputDir", _clipOutputDirPath},
            {"ConvertedOutputDir", _convertedOutputDir}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        _clipConverterService = new ClipConverterService(config);
    }

    [Test]
    public async Task ConvertMp4ToGif()
    {

        var fileName = "gss.mp4";
        var clipPath = Path.Combine(_clipOutputDirPath, fileName);
        var convertedClipPath = Path.ChangeExtension(Path.Combine(_convertedOutputDir, fileName), ".gif");
        if(!File.Exists(clipPath)) return;
        if(File.Exists(convertedClipPath)) File.Delete(convertedClipPath);

        await _clipConverterService.ConvertClipToGif(clipPath);

        Assert.True(File.Exists(convertedClipPath));

    }

}
