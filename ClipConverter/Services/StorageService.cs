using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClipConverter.Errors;
using ClipConverter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ClipConverter.Services;
public class StorageService : IStorageService
{

    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _blobContainerClient;
    private BlobContainerClient _convertedContainerClient;
    private IConfiguration _config;

    public StorageService(
        IConfiguration config,
        BlobServiceClient blobServiceClient
        )
    {
        _config = config;
        _blobServiceClient = blobServiceClient;
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(
            config.GetValue<string>("BlobContainerName"));

        _convertedContainerClient = _blobServiceClient.GetBlobContainerClient(
            config.GetValue<string>("ConvertedContainerName"));
    }

    public async Task<ServiceResult<string>> DownloadAsync(string fileName)
    {
        ServiceResult<string> serviceResult = new();
        try
        {
            var clipOutputPath = Path.Combine(_config.GetValue<string>("ClipsOutputDir"), fileName);
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DownloadToAsync(clipOutputPath);
            }
            serviceResult.Result = clipOutputPath;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult; ;
    }


    public async Task<ServiceResult<string>> UploadAsync(ConvertedClip convertedClip)
    {
        ServiceResult<string> serviceResult = new();
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

            serviceResult.Result = blobClient.Uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult;
    }

    public async Task<ServiceResult<string>> UploadHtmlAsync(string html, string id)
    {
         ServiceResult<string> serviceResult = new();
        try
        {
            BlobClient blobClient = _convertedContainerClient.GetBlobClient(id + ".html");
            Azure.Response<BlobContentInfo> response;
            await using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(html)))
            {
                response = await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "text/html" });
            }

            if (response.GetRawResponse().IsError)
                throw new Exception(response.GetRawResponse().ReasonPhrase);

            serviceResult.Result = blobClient.Uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult;
    }
}
