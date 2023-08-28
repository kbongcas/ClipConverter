
using ClipConverter.Dtos;
using ClipConverter.Errors;

namespace ClipConverter.Services;
public interface IClipService
{
    Task<ServiceResult<string>> EditClipUriAsync(EditClipUriRequestDto editClipUriRequestDto);
}