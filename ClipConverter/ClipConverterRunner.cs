using ClipConverter.Dtos;
using ClipConverter.Errors;
using ClipConverter.Models;
using ClipConverter.Services;
using ClipConverter.Services;
using Newtonsoft.Json;

namespace ClipConverter;

public class ClipConverterRunner
{
    private readonly QueueService _queueService;
    private readonly StorageService _storageService;
    private readonly ClipConverterService _clipConverterService;
    private readonly ClipService _clipService;

    public ClipConverterRunner(
        QueueService queueService,
        StorageService storageService,
        ClipConverterService clipConverterService,
        ClipService clipService
        )

    {
        _clipConverterService = clipConverterService;
        _clipService = clipService;
        _queueService = queueService;
        _storageService = storageService;
    }

    public async Task Run()
    {
        QueueMessageResultDto queueMessageResult = null;
        try
        {

            var peekMessageServiceResult = await _queueService.PeekMessageAsync();
            if (peekMessageServiceResult.IsError) throw new Exception(peekMessageServiceResult.ErrorMessage);
            queueMessageResult = peekMessageServiceResult.Result;

            //@TODO add validatons to deserialization
            var queueMessage = JsonConvert.DeserializeObject<QueueMessage>(peekMessageServiceResult?.Result?.Message ?? "");

            Console.WriteLine($"Downloading Clip {queueMessage.ClipId} from user {queueMessage.ClipUserId} ");
            var downloadServiceResult = await _storageService.DownloadAsync(queueMessage.ClipId);
            if (downloadServiceResult.IsError) throw new Exception(downloadServiceResult.ErrorMessage);

            Console.WriteLine($"Performing conversion on Clip {queueMessage.ClipId} from user {queueMessage.ClipUserId} ");
            var convertServiceResult = await _clipConverterService.ConvertClipToGif(downloadServiceResult.Result);
            if (convertServiceResult.IsError) throw new Exception(convertServiceResult.ErrorMessage);

            ServiceResult<string> uploadServiceResult = null;
            using (var stream = File.OpenRead(convertServiceResult.Result))
            {
                Console.WriteLine($"Uploading {convertServiceResult.Result} to storage.");
                ConvertedClip convertedClip = new ConvertedClip() { 
                    Id = Path.GetFileName(convertServiceResult.Result), 
                    Data = stream 
                };
                uploadServiceResult = await _storageService.UploadAsync(convertedClip);
                if (uploadServiceResult.IsError) throw new Exception(uploadServiceResult.ErrorMessage);
            }

            Console.WriteLine($"Editing conversion status");
            EditClipUriRequestDto editClipUriRequestDto = new EditClipUriRequestDto()
            {
                UserId = queueMessage.ClipUserId,
                ClipId = queueMessage.ClipId,
                Uri = new Uri(uploadServiceResult.Result),
                Converted = true,
            };
            var editClipServiceResult = await _clipService.EditClipUriAsync(editClipUriRequestDto);
            if (editClipServiceResult.IsError) throw new Exception(editClipServiceResult.ErrorMessage);

            Console.WriteLine($"Removing Message from queue: {peekMessageServiceResult.Result.Id}");
            var removeMessageServiceResult = await _queueService.RemoveMessageAsync(peekMessageServiceResult.Result);
            if (removeMessageServiceResult.IsError) throw new Exception(removeMessageServiceResult.ErrorMessage);

            Console.WriteLine($"Full Conversion Pipeline Finished Successfully");
        }
        catch (Exception ex)
        {
            // @TODO - Implement logic for when conversion fails
            // - Perhaps but count in message of times attempted to convert and 
            // keep trying if < a threshold
            // - Add some notification when the conversion failed. Etc.
            if(queueMessageResult != null)
            {
                Console.WriteLine($"FAILURE: Removing Message from queue: {queueMessageResult.Id}");
                await _queueService.RemoveMessageAsync(queueMessageResult);
            }
            Console.WriteLine(ex.ToString());
        }
    }
}
