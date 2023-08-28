using ClipConverter.Errors;
using ClipConverter.Models;

namespace ClipConverter.Services;

public interface IStorageService
{
    public Task<ServiceResult<string>> DownloadAsync(string fileName);
    public Task<ServiceResult<string>> UploadAsync(ConvertedClip convertedClip);
}