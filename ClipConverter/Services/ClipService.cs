using ClipConverter.Dtos;
using ClipConverter.Errors;
using ClipConverter.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace ClipConverter.Services;

public class ClipService : IClipService
{

    IConfiguration _config;

    public ClipService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ServiceResult<string>> EditClipUriAsync(EditClipUriRequestDto editClipUriRequestDto)
    {
        ServiceResult<string> serviceResult = new();
        try
        {
            // Get token
            var tokenEndpoint = Environment.GetEnvironmentVariable("Auth0TokenEndpoint");
            var tokenRestClient = new RestClient(tokenEndpoint);
            var tokenRequest = new RestRequest();

            tokenRequest.AddHeader("content-type", "application/json");
            tokenRequest.AddBody(new
            {
                client_id = Environment.GetEnvironmentVariable("Auth0ClientId"),
                client_secret = Environment.GetEnvironmentVariable("Auth0ClientSecret"),
                audience = Environment.GetEnvironmentVariable("Auth0Audience"),
                grant_type = "client_credentials"
            });

            var tokenResponse = await tokenRestClient.ExecutePostAsync(tokenRequest);

            if (tokenResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(tokenResponse.StatusCode.ToString());

            // Post Clip
            var token = JsonConvert.DeserializeObject<TokenResponseDto>(tokenResponse.Content!);
            var baseUrl = Environment.GetEnvironmentVariable("ClipsServiceEndpoint");
            var uri = new Uri(
                Uri.EscapeUriString($"{baseUrl}/{editClipUriRequestDto.UserId}/clips/{editClipUriRequestDto.ClipId}/uri")
                );
            var clipsClient = new RestClient(uri);

            var clipsRequest = new RestRequest();
            clipsRequest.Method = Method.Patch;
            clipsRequest.AddHeader("content-type", "application/json");
            clipsRequest.AddHeader("Authorization", $"{token.TokenType} {token.AccessToken}");
            clipsRequest.AddBody(new
            {
                uri = editClipUriRequestDto.Uri,
                converted = editClipUriRequestDto.Converted
            });
            var clipsResponse = await clipsClient.ExecuteAsync(clipsRequest);

            if (clipsResponse.StatusCode != System.Net.HttpStatusCode.OK)
                // @TODO - better execption handling here errormessage is null
                throw new Exception(clipsResponse.ErrorMessage.ToString());

            Console.WriteLine(clipsResponse.Content);
            var editClipUriResponseDto = JsonConvert.DeserializeObject<EditClipUriResponseDto>(clipsResponse.Content);

            if (editClipUriResponseDto?.Id == null)
                throw new Exception("There was a problem Adding a clip, reponse did not provide an Id.");

            serviceResult.Result = editClipUriResponseDto.Id;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }

        return serviceResult;
    }
}
