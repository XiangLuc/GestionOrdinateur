using System.Text.Json;

namespace AuthentificationApi;

public class KeycloakAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KeycloakAuthService> _logger;

    public KeycloakAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<KeycloakAuthService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync(string login, string password)
    {
        var baseUrl = _configuration["Keycloak:BaseUrl"];
        var realm = _configuration["Keycloak:Realm"];

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync(
            $"{baseUrl}/realms/{realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _configuration["Keycloak:ClientId"] ?? "",
                ["client_secret"] = _configuration["Keycloak:ClientSecret"] ?? "",
                ["username"] = login,
                ["password"] = password
            }));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Keycloak a refusé la connexion de {login} ({(int)response.StatusCode})");
            return null;
        }

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("access_token").GetString();
    }
}