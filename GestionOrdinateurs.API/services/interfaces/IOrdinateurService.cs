using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IOrdinateurService
{
    Task AjouterOrdinateur(Ordinateur ordinateur);
    Task<List<Ordinateur>> RecupererOrdinateurs();
    Task<Ordinateur?> RecupererOrdinateurParId(int id);
    Task<bool> SupprimerOrdinateurParId(int id);
    Task<bool> MettreAJourOrdinateur(int id, Ordinateur ordinateur);
    Task<List<Ordinateur>> ChercherOrdinateurParMarque(string marque);
    Task<List<Ordinateur>> RecupererOrdinateursDisponibles();
    Task<List<Ordinateur>> RecupererOrdinateursParPrixDecroissant();
    string Diagnostiquer(Ordinateur ordinateur);
    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();

    event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
}