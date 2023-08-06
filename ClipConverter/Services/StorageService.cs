using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClipConverter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClipConverter.Services;
public class StorageService : IStorageService
{

    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _blobContainerClient;
    private BlobContainerClient _convertedContainerClient;

    public StorageService(
        IConfiguration config,
        BlobServiceClient blobServiceClient
        )
    {
        _blobServiceClient = blobServiceClient;
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(
            config.GetValue<string>("BlobContainerName"));

        _convertedContainerClient = _blobServiceClient.GetBlobContainerClient(
            config.GetValue<string>("ConvertedContainerName"));
    }

    public async Task<Blob> GetFileAsync(string fileName)
    {
        // @TODO - implement error handleing
        // - handle error is file cannot be found in container
        Blob blob = new Blob();
        BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
        if (await blobClient.ExistsAsync())
        {
            var data = await blobClient.OpenReadAsync();

            var content = await blobClient.DownloadContentAsync();

            blob.Content = data;
            blob.Name = fileName;
            blob.ContentType = content.Value.Details.ContentType;
            return blob;
        }

        return blob;
    }

    public async Task UploadAsync(ConvertedClip convertedClip)
    {
        // @TODO - implement error handleing
        try
        {
            BlobClient blobClient = _convertedContainerClient.GetBlobClient(convertedClip.Id);
            Azure.Response<BlobContentInfo> response;
            await using (convertedClip.Data)
            {
                response = await blobClient.UploadAsync(convertedClip.Data);
            }

            if (response.GetRawResponse().IsError)
                throw new Exception(response.GetRawResponse().ReasonPhrase);
        }
        catch (Exception ex)
        {
            //@TODO - Log Error Message
        }
    }
}
