using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ClipConverter.Dtos;
using ClipConverter.Services;
using Microsoft.Extensions.Configuration;

namespace ClipUploaderTests.Services;
public class QueueServiceTests
{
    private QueueService _queueService;
    private QueueClient _queueClient;


    [SetUp]
    public void SetUp()
    {
        var queueName = "clips";
        var inMemConfig = new Dictionary<string, string> {
            { "QueueName", queueName }
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        var queueOpts = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
        var connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        QueueServiceClient queueServiceClient = new QueueServiceClient(connectionString, queueOpts);


        _queueClient = queueServiceClient.GetQueueClient("clips");
        _queueService = new QueueService(config, queueServiceClient);
    }

    [Test]
    public async Task PeekMessageTest()
    {
        // single message
        await _queueClient.ClearMessagesAsync();
        var guid = Guid.NewGuid().ToString();
        await _queueClient.SendMessageAsync(guid);

        var queueMessageData = await _queueService.PeekMessageAsync();

        Assert.IsNotNull(queueMessageData);
        Assert.That(queueMessageData.Result.Message, Is.EqualTo(guid));

        // multiple messages
        await _queueClient.ClearMessagesAsync();
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        var guid3 = Guid.NewGuid().ToString();
        await _queueClient.SendMessageAsync(guid1);
        await _queueClient.SendMessageAsync(guid2);
        await _queueClient.SendMessageAsync(guid3);

        var queueMessageData1 = await _queueService.PeekMessageAsync();

        Assert.IsNotNull(queueMessageData);
        Assert.That(queueMessageData1.Result.Message, Is.EqualTo(guid1));
    }

    [Test]
    public async Task DeleteMessageTest()
    {
        await _queueClient.ClearMessagesAsync();
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();
        await _queueClient.SendMessageAsync(guid1);
        var sendReceipt = await _queueClient.SendMessageAsync(guid2);
        QueueMessageResultDto queueMessageData = new QueueMessageResultDto()
        {
            Id = sendReceipt.Value.MessageId,
            PopReceipt = sendReceipt.Value.PopReceipt,
            Message = guid2
        };

        await _queueService.RemoveMessageAsync(queueMessageData);

        PeekedMessage[] messages = await _queueClient.PeekMessagesAsync();
        Assert.IsNotNull(messages);
        var message = messages.FirstOrDefault(m => m.MessageId == queueMessageData.Id);
        Assert.IsNull(message);
    }
}
