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
    private readonly string outputDirPath = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/";

    [SetUp]
    public void SetUp()
    {
        var inMemConfig = new Dictionary<string, string> {
            {"FfmpegExecPath", ffmpegPath},
            {"ConvertedClipOutputDir", outputDirPath}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        _clipConverterService = new ClipConverterService(config);
    }

    [Test]
    public async Task ConvertMp4ToGif()
    {

        var fileName = "gs.mp4";
        var filePath = Path.Combine(Environment.CurrentDirectory, "Data/") + fileName;
        var blobName = Guid.NewGuid().ToString();
        MediaFile mediaFile = null;
        using (var stream = System.IO.File.OpenRead(filePath))
        {
            Blob blob = new Blob() { Name = blobName, Content = stream };
            mediaFile = await _clipConverterService.ConvertClipToGif(blob);
        }

        Assert.NotNull(mediaFile);

    }

}
