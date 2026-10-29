using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IReparationService
{
    Task<Reparation> AjouterReparation(Reparation reparation);
    Task SimulerReparationAsync(Ordinateur ordinateur, string technicien);
    Task<List<Reparation>> RecupererReparations();
    Task<List<ReparationService.HistoriqueReparationRecord>> RecupererHistorique();
}
