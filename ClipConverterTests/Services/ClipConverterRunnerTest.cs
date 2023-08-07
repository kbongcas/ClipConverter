using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using ClipConverter.Services;
using Microsoft.Extensions.Configuration;
using ClipConverter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace ClipConverterTests.Services;

public class ClipConverterRunnerTest
{

    private QueueService _queueService;
    private StorageService _storageService;
    private BlobServiceClient _blobServiceClient;
    private QueueServiceClient _queueServiceClient;
    private ClipConverterService _clipConverterService;
    private QueueClient _queueClient;
    private ClipConverterRunner _clipConverterRunner;
    public string ffmpegPath = "C:\\ProgramData\\chocolatey\\bin\\ffmpeg.exe";
    public string outputDirPath = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/";
    public string clipsOutputDir = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/clips/";
    public string convertedOutputDir = "C:/Users/kbong/projects/dotnet/ClipConverter/ClipConverterTests/Data/converted/";
    public string queueName = "clips";
    public string clipsContainerName = "clips";
    public string convertedClipsContainerName = "convertedclips";


    [SetUp]
    public void SetUp()
    {

        var inMemConfig = new Dictionary<string, string> {

            {"FfmpegExecPath", ffmpegPath},
            {"ConvertedClipOutputDir", convertedOutputDir},
            {"ClipsOutputDir", clipsOutputDir},
            {"QueueName", queueName },
            {"BlobContainerName", clipsContainerName},
            {"ConvertedContainerName", convertedClipsContainerName}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        var connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        var queueOpts = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };

        _blobServiceClient = new BlobServiceClient(connectionString);
        _storageService = new StorageService(config, _blobServiceClient);

        _queueServiceClient = new QueueServiceClient(connectionString, queueOpts);
        _queueClient = _queueServiceClient.GetQueueClient("clips");
        _queueService = new QueueService(config, _queueServiceClient);

        _clipConverterService = new ClipConverterService(config);

        _clipConverterRunner = new ClipConverterRunner(
            _queueService,
            _storageService,
            _clipConverterService
            );
    }

    [Test]
    public async Task RunTest()
    {
        var fileName = "gs.mp4";
        var filePath = Path.Combine(Environment.CurrentDirectory, "Data/") + fileName;

        // add file to clip storage
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(clipsContainerName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
        if (blobClient.Exists()) await blobClient.DeleteAsync();
        using (var stream = File.OpenRead(filePath))
        {
            await blobClient.UploadAsync(stream);
        }

        //add message to queue
        await _queueClient.ClearMessagesAsync();
        var guid2 = Guid.NewGuid().ToString();
        await _queueClient.SendMessageAsync(fileName);
        await _queueClient.SendMessageAsync(guid2);


        await _clipConverterRunner.Run();

        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        var messages = await queueClient.PeekMessagesAsync(5);

        // test if item gets removed form queue
        Assert.That(messages.Value.Length, Is.EqualTo(1));
        var message = messages.Value.FirstOrDefault(m => m.MessageId == fileName);
        Assert.IsNull(message);

        // test if converted file is inserted in converted container
        var convertedContainerClient = _blobServiceClient.GetBlobContainerClient(convertedClipsContainerName);
        BlobClient newBlobClient = convertedContainerClient.GetBlobClient(Path.ChangeExtension(fileName, ".gif"));
        Assert.True(await newBlobClient.ExistsAsync());

    }
}
