
using Azure;
using ClipConverter.Dtos;
using ClipConverter.Errors;

namespace ClipConverter.Services;

public interface IQueueService
{
    public Task<ServiceResult<QueueMessageResultDto>> PeekMessageAsync();
    public Task<ServiceResult<Response>> RemoveMessageAsync(QueueMessageResultDto queueMessageResultDto);
}