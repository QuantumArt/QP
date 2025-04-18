using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using NLog;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.KeyCloak;

public class KeyCloakService : IKeyCloakSyncService, ISsoAuthService
{
    private readonly IKeyCloakApiHelper _apiHelper;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly KeyCloakSettings _settings;

    private const string GroupPathTemplate = "admin/realms/{0}/groups?search={1}";
    private const string UsersInGroupPathTemplate = "admin/realms/{0}/groups/{1}/members";
    private const string AuthenticateUrlTemplate = "{0}/realms/{1}/protocol/openid-connect/auth?response_type={2}&client_id={3}&redirect_uri={4}&scope={5}&state={6}&code_challenge={7}&code_challenge_method=S256";
    private const string ResponseType = "code";
    private const string Scope = "openid";

    public KeyCloakService(IKeyCloakApiHelper apiHelper, IOptions<KeyCloakSettings> settings)
    {
        _apiHelper = apiHelper;
        _settings = settings.Value;
    }

    public async Task<List<KeyCloakGroup>> GetGroups(List<string> groupNames)
    {
        List<KeyCloakGroup> result = new();

        foreach (string groupName in groupNames)
        {
            KeyCloakGroup[] groups = await _apiHelper.GetAsync<KeyCloakGroup[]>(string.Format(GroupPathTemplate, _settings.Realm, groupName));

            if (groups.Length == 0)
            {
                _logger.ForInfoEvent()
                    .Message("KeyCloak group not found.")
                    .Property("GroupName", groupName)
                    .Log();

                continue;
            }

            KeyCloakGroup foundGroup = RetrieveGroup(groups.First(), groupName);

            if (foundGroup == null)
            {
                continue;
            }

            result.Add(foundGroup);
        }

        return result;
    }

    public async Task<Dictionary<string, List<KeyCloakUser>>> GetUsers(List<KeyCloakGroup> keyCloakGroups)
    {
        Dictionary<string, List<KeyCloakUser>> result = new();

        foreach (KeyCloakGroup keyCloakGroup in keyCloakGroups)
        {
            KeyCloakUser[] users = await _apiHelper.GetAsync<KeyCloakUser[]>(string.Format(UsersInGroupPathTemplate, _settings.Realm, keyCloakGroup.Id));
            result.Add(keyCloakGroup.Name, users.ToList());
        }

        return result;
    }

    public async Task<SsoAuthResult> CheckUserAuth(string code, string verifier)
    {
        KeyCloakAuthResult result = await _apiHelper.CheckAuthorization(code, verifier);

        SsoAuthResult authResultInfo = new()
        {
            IsSuccess = result.IsSuccess
        };

        if (!result.IsSuccess)
        {
            authResultInfo.Error = result.Response.GetProperty("error").GetString();
        }
        else
        {
            string idToken = result.Response.GetProperty("id_token").GetString();
            JsonWebTokenHandler handler = new();
            JsonWebToken token = handler.ReadJsonWebToken(idToken);

            if (token.TryGetPayloadValue("preferred_username", out string username))
            {
                authResultInfo.UserName = username;
            }
            else
            {
                authResultInfo.IsSuccess = false;
                authResultInfo.Error = "Username not found in id token";
            }
        }

        return authResultInfo;
    }

    public string GenerateCodeVerifier(int length = 32)
    {
        byte[] randomBytes = new byte[length];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Base64UrlEncode(randomBytes);
    }

    public string GenerateCodeChallenge(string codeVerifier)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    public string GetAuthenticateUrl(string state, string challenge)
    {
        return string.Format(AuthenticateUrlTemplate,
            _settings.ApiUrl,
            _settings.Realm,
            ResponseType,
            _settings.AuthClientId,
            _settings.RedirectAddress,
            Scope,
            state,
            challenge);
    }


    public async Task<bool> CheckSsoEnabled(string customerCode)
    {
        try
        {
            QPContext.CurrentCustomerCode = customerCode;

            string ssoEnabledString = await QPContext.EFContext.AppSettingsSet
                .Where(x => x.Key == _settings.EnableSettingName)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            return bool.TryParse(ssoEnabledString, out bool ssoEnabled) && ssoEnabled;
        }
        catch (Exception ex)
        {
            _logger.ForErrorEvent()
                .Exception(ex)
                .Message("Error while checking SSO enabled")
                .Property("CustomerCode", customerCode)
                .Log();

            return false;
        }

    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static KeyCloakGroup RetrieveGroup(KeyCloakGroup group, string groupName)
    {
        KeyCloakGroup result = null;

        if (group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
        {
            group.SubGroups = Array.Empty<KeyCloakGroup>();
            result = group;

            return result;
        }

        foreach (KeyCloakGroup subGroup in group.SubGroups)
        {
            result = RetrieveGroup(subGroup, groupName);

            if (result != null)
            {
                return result;
            }
        }

        return result;
    }
}
