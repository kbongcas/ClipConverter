
using ClipConverter.Models;

namespace ClipConverter.Services;

public interface IQueueService
{
    public Task<QueueMessageData> PeekMessageAsync();
    public Task RemoveMessageAsync(QueueMessageData queueMessageData);
}