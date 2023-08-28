using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ClipConverter.Dtos;
using ClipConverter.Errors;
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

    public async Task<ServiceResult<QueueMessageResultDto>> PeekMessageAsync()
    {
        ServiceResult<QueueMessageResultDto> serviceResult = new();
        try
        {
            // @TODO - Implement Error Handling
            QueueMessageResultDto queueMessageResultDto = new QueueMessageResultDto();
            var response = await _queueClient.ReceiveMessageAsync();
            if (response.Value == null) throw new Exception("There was a problem reading the queue.");

            var message = response.Value.Body.ToString();
            queueMessageResultDto.Id = response.Value.MessageId;
            queueMessageResultDto.PopReceipt = response.Value.PopReceipt;
            queueMessageResultDto.Message = message;

            serviceResult.Result = queueMessageResultDto;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult;
    }

    public async Task<ServiceResult<Response>> RemoveMessageAsync(QueueMessageResultDto queueMessageData)
    {
        ServiceResult<Response> serviceResult = new();
        try
        {
            serviceResult.Result = await _queueClient.DeleteMessageAsync(queueMessageData.Id, queueMessageData.PopReceipt);
        }
        catch(Exception ex) 
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult;
    }
}
