using ClipConverter.Models;

namespace ClipConverter.Services;

public interface IStorageService
{
    public Task<Blob> GetFileAsync(string fileName);
    public Task<string> DownloadAsync(string fileName);
}