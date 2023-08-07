using ClipConverter.Models;

namespace ClipConverter.Services;
public interface IClipConverterService
{
    Task<string> ConvertClipToGif(string fileName);
}