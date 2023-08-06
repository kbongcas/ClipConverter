using ClipConverter.Models;
using FFmpeg.NET;

namespace ClipConverter.Services;
public interface IClipConverterService
{
    Task<MediaFile> ConvertClipToGif(Blob blob);
}