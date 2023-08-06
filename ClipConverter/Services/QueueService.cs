using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ClipConverter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace ClipConverter.Services;

public class QueueService : IQueueService
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly QueueClient _queueClient;

    public QueueService(
        IConfiguration config,
        QueueServiceClient queueServiceClient
        )
    {
        _queueServiceClient = queueServiceClient;
        _queueClient = _queueServiceClient.GetQueueClient(config.GetValue<string>("QueueName"));
    }

    public async Task<QueueMessageData> PeekMessageAsync()
    {
        // @TODO - Implement Error Handling
        QueueMessageData blobUri = new QueueMessageData();
        var response = await _queueClient.ReceiveMessageAsync();
        if ( response.Value != null ) {
            var message = response.Value.Body.ToString();
            blobUri.Id = response.Value.MessageId;
            blobUri.PopReceipt = response.Value.PopReceipt;
            blobUri.Message = message;
            return blobUri;
        }
        return blobUri;
    }

    public async Task RemoveMessageAsync(QueueMessageData queueMessageData)
    {
        // @TODO - Implement Error Handling
        await _queueClient.DeleteMessageAsync(queueMessageData.Id, queueMessageData.PopReceipt);
    }
}
