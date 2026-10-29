using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class OrdinateurService : IOrdinateurService
{
    private readonly ParcInformatiqueContext _context;
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;

    public OrdinateurService(ParcInformatiqueContext context)
    {
        _context = context;
    }

    public async Task AjouterOrdinateur(Ordinateur ordinateur)
    {
        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync();
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs
        {
            Nom = ordinateur.Nom
        });
    }

    public async Task<List<Ordinateur>> RecupererOrdinateurs()
    {
        return await _context.Ordinateurs.ToListAsync();
    }

    public async Task<Ordinateur?> RecupererOrdinateurParId(int id)
    {
        var ordinateurs = await _context.Ordinateurs
        .Where(o => o.Id == id)
        .ToListAsync();

        return ordinateurs.FirstOrDefault();
    }

    public async Task<bool> SupprimerOrdinateurParId(int id)
    {
        Ordinateur? ordinateur = await RecupererOrdinateurParId(id);
        if (ordinateur == null)
        {
            return false;
        }

        _context.Ordinateurs.Remove(ordinateur);
        await _context.SaveChangesAsync();
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs
        {
            Id = ordinateur.Id,
            Nom = ordinateur.Nom
        });
        return true;
    }

    public async Task<bool> MettreAJourOrdinateur(int id, Ordinateur ordinateur)
    {
        Ordinateur? existant = await RecupererOrdinateurParId(id);
        if (existant == null)
        {
            return false;
        }

        existant.Nom = ordinateur.Nom;
        existant.Etat = ordinateur.Etat;
        existant.Caracteristiques = ordinateur.Caracteristiques;
        existant.Stock = ordinateur.Stock;
        existant.Prix = ordinateur.Prix;

        await _context.SaveChangesAsync();
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs
        {
            Id = existant.Id,
            Nom = existant.Nom
        });
        return true;
    }

    public async Task<List<Ordinateur>> ChercherOrdinateurParMarque(string marque)
    {
        return await _context.Ordinateurs
        .Where(o => o.Caracteristiques.Marque == marque)
            .ToListAsync();
    }

    public async Task<List<Ordinateur>> RecupererOrdinateursDisponibles()
    {
        return await _context.Ordinateurs
        .Where(o => o.Stock > 0)
        .ToListAsync();
    }

    public async Task<List<Ordinateur>> RecupererOrdinateursParPrixDecroissant()
    {
        var ordinateurs = await _context.Ordinateurs.ToListAsync();

        return ordinateurs
            .OrderByDescending(o => o.Prix)
            .ToList();
    }

    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            {
                Etat: EtatOrdinateur.HorsService
            }
                => "Remplacement",

            {
                Etat: EtatOrdinateur.Lent,
                Caracteristiques:
                {
                    RamGo: < 8
                }
            }
                => "Ajouter RAM",

            _
                => "Aucune action"
        };
    }

    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)
        CalculerStatistiques()
    {
        var ordinateurs = _context.Ordinateurs.ToList();

        return (
            ordinateurs.Count,
            ordinateurs.Count(o => o.Stock > 0),
            ordinateurs.Sum(o => o.Prix * o.Stock),
            _context.Reparations.Count()
        );
    }
}