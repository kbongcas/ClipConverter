using ClipConverter.Models;

namespace ClipConverter.Services;
public interface IClipConverterService
{
    Task<Errors.ServiceResult<string>> ConvertClipToGif(string fileName);
}