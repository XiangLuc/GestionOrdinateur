using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly KeycloakAuthService _keycloakAuthService;

    public AuthenticationController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthenticationController> logger, KeycloakAuthService keycloakAuthService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _keycloakAuthService = keycloakAuthService;
    }

    [HttpPost("login-keycloak")]
    public async Task<IActionResult> LoginKeycloak([FromBody] LoginRequest request)
    {
        var token = await _keycloakAuthService.GetTokenAsync(request.Login, request.Password);

        if (token == null)
        {
            return Unauthorized("Login ou mot de passe incorrect");
        }

        _logger.LogInformation($"Token Keycloak généré pour l'utilisateur {request.Login}");
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string login)
    {
        var client = _httpClientFactory.CreateClient("GestionOrdinateursApi");
        var response = await client.GetAsync($"users/userbylogin/{login}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Connexion refusée : l'utilisateur {login} n'existe pas");
            return NotFound($"L'utilisateur {login} n'existe pas");
        }

        var user = await response.Content.ReadFromJsonAsync<User>();

        var jwtKey = _configuration["Jwt:Key"] ?? "";
        var key = Encoding.ASCII.GetBytes(jwtKey);

        _logger.LogInformation($"Token généré pour l'utilisateur {user!.Login} avec le rôle {user.Role}");
        return Ok(GenTokenJWT(key, user));
    }

    private string GenTokenJWT(byte[] keyP, User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyP), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}