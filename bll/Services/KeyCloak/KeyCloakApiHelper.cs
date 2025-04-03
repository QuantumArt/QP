using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakApiHelper : IKeyCloakApiHelper
{
    private const string TokenEndpointFormat = "realms/{0}/protocol/openid-connect/token";

    private readonly IHttpClientFactory _clientFactory;
    private readonly KeyCloakSettings _settings;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private KeyCloakToken _token;

    public KeyCloakApiHelper(IHttpClientFactory clientFactory, IOptions<KeyCloakSettings> settings)
    {
        _clientFactory = clientFactory;
        _settings = settings.Value;
    }

    private async Task<string> GetTokenAsync()
    {
        if (_token is { Expired: false })
        {
            return _token.Token;
        }

        FormUrlEncodedContent content = new(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        });

        try
        {
            using HttpClient client = _clientFactory.CreateClient(KeyCloakSettings.HttpClientName);
            HttpResponseMessage response = await client.PostAsync(string.Format(TokenEndpointFormat, _settings.Realm), content);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JsonElement tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);

            _token = new()
            {
                Token = tokenResponse.GetProperty("access_token").GetString(),
                Expiration = tokenResponse.GetProperty("expires_in").GetInt32()
            };

            return _token.Token;
        }
        catch (HttpRequestException ex)
        {
            _logger.ForErrorEvent()
                .Message("Error while getting token from KeyCloak")
                .Exception(ex)
                .Log();

            throw;
        }
    }

    public async Task<T> GetAsync<T>(string apiUrl)
    {
        try
        {
            string accessToken = await GetTokenAsync();

            using HttpClient client = _clientFactory.CreateClient(KeyCloakSettings.HttpClientName);
            client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (HttpRequestException ex)
        {
            _logger.ForErrorEvent()
                .Message("Error while getting data from KeyCloak")
                .Exception(ex)
                .Log();

            throw;
        }
    }

    public async Task<KeyCloakAuthResult> CheckAuthorization(string code, string verifier)
    {
        try
        {
            FormUrlEncodedContent content = new(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "code_verifier", verifier },
                { "client_id", _settings.AuthClientId },
                { "redirect_uri", _settings.RedirectAddress }
            });

            using HttpClient client = _clientFactory.CreateClient(KeyCloakSettings.HttpClientName);
            HttpResponseMessage response = await client.PostAsync(string.Format(TokenEndpointFormat, _settings.Realm), content);
            string responseBody = await response.Content.ReadAsStringAsync();

            return new()
            {
                Response = JsonSerializer.Deserialize<JsonElement>(responseBody),
                IsSuccess = response.IsSuccessStatusCode
            };
        }
        catch (Exception e)
        {
            _logger.ForErrorEvent()
                .Message("Error while checking authorization")
                .Exception(e)
                .Log();

            return new();
        }
    }
}
