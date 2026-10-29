using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService service, ILogger<UsersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("userbylogin/{login}")]
    public async Task<IActionResult> ChercherUtilisateurParLogin(string login)
    {
        var user = await _service.ChercherUtilisateurParLogin(login);
        if (user == null)
        {
            _logger.LogWarning($"Utilisateur {login} introuvable");
            return NotFound();
        }
        _logger.LogInformation($"Utilisateur {login} trouvé");
        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> RecupererUtilisateurs()
    {
        _logger.LogInformation("Récupération de la liste des utilisateurs");
        return Ok(await _service.RecupererUtilisateurs());
    }
}
