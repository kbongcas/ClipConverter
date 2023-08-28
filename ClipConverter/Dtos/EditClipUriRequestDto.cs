using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace ClipConverter.Dtos;
public class EditClipUriRequestDto
{
    public string UserId { get; set; }

    public string ClipId { get; set; }

    [JsonProperty("uri")]
    public Uri? Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }
}
