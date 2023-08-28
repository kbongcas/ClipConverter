
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace ClipConverter.Dtos;

public class EditClipUriResponseDto
{
    [JsonProperty("id")]
    public string Id { get;}

    [JsonProperty("uri")]
    public Uri? Uri { get; }

    [JsonProperty("converted")]
    public bool Converted { get; }
}
