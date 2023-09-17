using Azure;
using Azure.Storage.Blobs;
using ClipConverter.Dtos;
using ClipConverter.Models;
using ClipConverter.Services;
using ClipConverter.Utils;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ClipDataServiceTests.Services;
public class StorageServiceTests
{
    private StorageService _storageService;
    private BlobServiceClient _blobServiceClient;
    private readonly string _clipsContainerName = "clips";
    private readonly string _convertedClipsContainerName = "convertedclips";
    private string _clipsOutputDir = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/clips";


    [SetUp]
    public void SetUp()
    {
        // local storage account    
        var connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        _blobServiceClient = new BlobServiceClient(connectionString);

        var inMemConfig = new Dictionary<string, string> {
            {"BlobContainerName", _clipsContainerName},
            {"ConvertedContainerName", _convertedClipsContainerName},
            { "ClipsOutputDir" , _clipsOutputDir }
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();
        _storageService = new StorageService(config, _blobServiceClient);

    }

    [Test]
    public async Task DownloadTest()
    {
        var fileName = "gs.mp4";
        var filePath = Path.Combine(Environment.CurrentDirectory + "/Data", fileName);
        var resultFilePath = Path.Combine(_clipsOutputDir, fileName);
        if(File.Exists(resultFilePath)) File.Delete(resultFilePath);

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_clipsContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        if (blobClient.Exists()) await blobClient.DeleteAsync();
        using (var stream = System.IO.File.OpenRead(filePath))
        {
            await blobClient.UploadAsync(stream);
        }

        await _storageService.DownloadAsync(fileName);

        Assert.True(File.Exists(resultFilePath));

    }

    [Test]
    public async Task UploadTest()
    {
        var fileName = "gs.mp4";
        var filePath = Path.Combine(Environment.CurrentDirectory, "Data/") + fileName;
        var newFileName = Guid.NewGuid().ToString();

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_convertedClipsContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(newFileName);

        if (blobClient.Exists()) await blobClient.DeleteAsync();


        var stream = System.IO.File.OpenRead(filePath);

        ConvertedClip convertedClip = new ConvertedClip()
        {
            Id = newFileName,
            Data = stream,
        };

        await _storageService.UploadAsync(convertedClip);


        BlobClient newBlobClient = containerClient.GetBlobClient(newFileName);
        Assert.True(await newBlobClient.ExistsAsync());

    }
    [Test]
    public async Task UploadHtmlTest()
    {

        var newFileName = Guid.NewGuid().ToString();
        GenerateHtmlRequestDto generateHtmlRequestDto = new GenerateHtmlRequestDto()
        {
            Name = "testname",
            Description = "testdescript",
            ConvertedFile = "https://clipdatsa.blob.core.windows.net/converted/0b579e83-0384-4675-b218-2bb9cda4eba5.gif",
        };
        var htmlString = ClipHtmlGenerator.GenerateHtml(generateHtmlRequestDto);


        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_convertedClipsContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(newFileName+".html");

        if (blobClient.Exists()) await blobClient.DeleteAsync();

        await _storageService.UploadHtmlAsync(htmlString,newFileName);

        BlobClient newBlobClient = containerClient.GetBlobClient(newFileName + ".html");
        Assert.True(await newBlobClient.ExistsAsync());

    }

}