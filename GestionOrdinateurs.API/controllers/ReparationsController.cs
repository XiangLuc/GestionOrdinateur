using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReparationsController : ControllerBase
{
    private readonly IReparationService _reparService;
    private readonly IOrdinateurService _ordinateurService;
    private readonly ILogger<ReparationsController> _logger;
    private readonly IStockService _stockService;

    public ReparationsController(IReparationService reparService, IOrdinateurService ordinateurService, ILogger<ReparationsController> logger, IStockService stockService)
    {
        _reparService = reparService;
        _ordinateurService = ordinateurService;
        _logger = logger;
        _stockService = stockService;
    }

    [HttpPost("SimulerReparation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SimulerReparation()
    {
        List<Ordinateur> listeOrdi = await _ordinateurService.RecupererOrdinateurs();

        if (listeOrdi.Count < 5)
        {
            _logger.LogWarning($"erreur lors de la simulation car ordinateur < 5");
            return BadRequest();
        }

        await Task.WhenAll(
            _reparService.SimulerReparationAsync(listeOrdi[0], "Alice"),
            _reparService.SimulerReparationAsync(listeOrdi[1], "Bruno"),
            _reparService.SimulerReparationAsync(listeOrdi[2], "Chloé"),
            _reparService.SimulerReparationAsync(listeOrdi[3], "Alice"),
            _reparService.SimulerReparationAsync(listeOrdi[4], "Bruno")
        );

        _logger.LogInformation("la simulation des 5 réparations est terminée");
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> RecupererReparations()
    {
        var reparations = await _reparService.RecupererReparations();
        _logger.LogInformation($"Ordinateurs trouvé : {reparations.Count}");
        return Ok(reparations);
    }

    [HttpGet("historique")]
    public async Task<IActionResult> RecupererHistorique()
    {
        var historique = await _reparService.RecupererHistorique();
        _logger.LogInformation($"Historique des réparations : {historique.Count} trouvé(s)");
        return Ok(historique);
    }

    [HttpGet("GetStockSSD")]
    public int GetStockSSD()
    {
        Parallel.For(
            0,
            10,
            i => _stockService.ConsommerSSD());

        _logger.LogInformation($"StockSSD : {_stockService.StockSSD}");
        return (_stockService.StockSSD);
    }

    /*
    Question 17 : Je constate qu'on fais qu'une tache à la fois (-1 par exécution) quand on exécute ce endpoint 
    alors que la décrémentation devrait retiré directement 10 de stocks et pas 1. Non ce n'est pas cohérent avec ce que la question
    veux faire, on devrait retiré 10 mais on retire 1.
    */

    /*
    Question 20 : Le threads exécute bien 10 fois la tache puis bloque quand il fini ces meme 10 exécutions grace au lock, 
    on a donc un résultat cohérent comparé à avant.
    */

}