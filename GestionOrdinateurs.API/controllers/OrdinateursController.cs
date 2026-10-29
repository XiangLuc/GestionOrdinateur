using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IConfiguration _configuration;
    private readonly JournalService _journal;
    private readonly ParcInformatiqueContext _context;

    public OrdinateursController(IOrdinateurService service, ILogger<OrdinateursController> logger, IConfiguration configuration, JournalService journal, ParcInformatiqueContext context)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;
        _journal = journal;
        _context = context;

        _service.OrdinateurAjoute += AfficherMessageAjout;
        _service.OrdinateurSupprime += AfficherMessageSuppression;
        _service.OrdinateurMisAJour += AfficherMessageMiseAJour;
    }

    [HttpGet]
    public async Task<IActionResult> RecupererOrdinateurs()
    {
        _logger.LogInformation($"Récupération de la liste des ordinateurs");
        return Ok(await _service.RecupererOrdinateurs());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> RecupererOrdinateurParId(int id)
    {
        var ordinateur = await _service.RecupererOrdinateurParId(id);
        if (ordinateur == null)
        {
            _logger.LogWarning($"Ordinateur {id} introuvable");
            return NotFound();
        }
        _logger.LogInformation($"Ordinateur {id} trouvé");
        return Ok(ordinateur);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AjouterOrdinateur([FromBody] Ordinateur ordinateur)
    {
        await _service.AjouterOrdinateur(ordinateur);
        _logger.LogInformation($"Ordinateur ajouté avec pour nom : {ordinateur.Nom} et marque : {ordinateur.Caracteristiques.Marque}");
        return Created();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SupprimerOrdinateur(int id)
    {
        var supprime = await _service.SupprimerOrdinateurParId(id);
        if (!supprime)
        {
            _logger.LogWarning($"Suppression impossible pour l'ordinateur {id}");
            return NotFound();
        }
        _logger.LogInformation($"Ordinateur avec l'id {id} supprimé");
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MettreAJourOrdinateur(int id, [FromBody] Ordinateur ordinateur)
    {
        var misAJour = await _service.MettreAJourOrdinateur(id, ordinateur);
        if (!misAJour)
        {
            _logger.LogWarning($"Mise à jour impossible car l'ordinateur avec l'id {id} est introuvable");
            return NotFound();
        }
        _logger.LogInformation($"L'ordinateur avec l'id {id} a été mise à jour");
        return Ok(await _service.RecupererOrdinateurParId(id));
    }

    [HttpGet("marque/{marque}")]
    public async Task<IActionResult> ChercherOrdinateurParMarque(string marque)
    {
        var ordinateurs = await _service.ChercherOrdinateurParMarque(marque);
        _logger.LogInformation($"Recherche par marque '{marque}' avec {ordinateurs.Count} résultats trouvé");
        return Ok(ordinateurs);
    }

    [HttpGet("info")]
    public IActionResult RecupererInfosApplication()
    {
        var infos = new
        {
            Nom = _configuration["Application:Name"],
            Version = _configuration["Application:Version"],
        };

        _logger.LogInformation($"Information de l'Application - Nom de l'App : {infos.Nom} et la version : {infos.Version}");
        return Ok(infos);
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> RecupererOrdinateursDisponibles()
    {
        var ordinateurs = await _service.RecupererOrdinateursDisponibles();
        _logger.LogInformation($"les ordinateurs disponibles : {ordinateurs.Count} trouvé(s)");
        return Ok(ordinateurs);
    }

    [HttpGet("prix")]
    public async Task<IActionResult> RecupererOrdinateursParPrixDecroissant()
    {
        var ordinateurs = await _service.RecupererOrdinateursParPrixDecroissant();
        _logger.LogInformation($"ordinateurs du plus cher au moins cher : {ordinateurs.Count} trouvé(s)");
        return Ok(ordinateurs);
    }

    [HttpGet("{id}/diagnostic")]
    public async Task<IActionResult> Diagnostiquer(int id)
    {
        var ordinateur = await _service.RecupererOrdinateurParId(id);
        if (ordinateur == null)
        {
            _logger.LogWarning($"Diagnostic impossible car l'ordinateur {id} est introuvable");
            return NotFound();
        }

        var diagnostic = _service.Diagnostiquer(ordinateur);
        _logger.LogInformation($"Diagnostic de l'ordinateur {id} : {diagnostic}");
        return Ok(new { ordinateur.Id, ordinateur.Nom, ordinateur.Etat, Diagnostic = diagnostic });
    }

    [HttpGet("statistiques")]
    public IActionResult RecupererStatistiques()
    {
        var (total, enStock, valeurStock, nbReparations)
            = _service.CalculerStatistiques();

        _logger.LogInformation($"Statistiques : {total} ordinateurs, {enStock} en stock, valeur {valeurStock}, {nbReparations} réparations");
        return Ok(new
        {
            TotalOrdinateur = total,
            OrdiDisponibles = enStock,
            ValeurStock = valeurStock,
            NbReparations = nbReparations
        });
    }

    [HttpGet("inventaire/{nom}")]
    public IActionResult Chercher(string nom)
    {
        var inventaire = new InventaireOrdinateurs();
        inventaire.Ordinateurs = _context.Ordinateurs.ToList();

        var ordinateur = inventaire[nom]; // Exemple de recherche par nom

        return Ok(ordinateur);
    }

    [HttpGet("inventaire/id/{id}")]
    public IActionResult ChercherParId(int id)
    {
        var inventaire = new InventaireOrdinateurs();
        inventaire.Ordinateurs = _context.Ordinateurs.ToList();

        var ordinateur = inventaire[id];

        return Ok(ordinateur);
    }

    // --------------------------------------------------------------------------------------------------
    private void AfficherMessageAjout(object? sender, OrdinateurAjouteEventArgs e)
    {
        Console.WriteLine($"Création d'un nouvel ordinateur : {e.Nom}");
        _journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {e.Nom} ajouté");
    }

    private void AfficherMessageSuppression(object? sender, OrdinateurSupprimeEventArgs e)
    {
        Console.WriteLine($"Suppression de l'ordinateur {e.Nom} avec l'Id : {e.Id}");
        _journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {e.Nom} supprimé");
    }

    private void AfficherMessageMiseAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        Console.WriteLine($"Mise à jour de l'ordinateur avec l'Id : {e.Id}");
        _journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {e.Nom} modifié");
    }
}