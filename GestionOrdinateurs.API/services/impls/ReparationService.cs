using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class ReparationService : IReparationService
{
    public record HistoriqueReparationRecord(
        int OrdinateurId,
        string NomOrdinateur,
        string Technicien,
        string TypeReparation,
        DateTime DateIntervention
    );

    private readonly ParcInformatiqueContext _context;
    private readonly JournalService _journal;
    private readonly object _reparationLock = new object();

    public ReparationService(ParcInformatiqueContext context, JournalService journal)
    {
        _context = context;
        _journal = journal;
    }

    public async Task<Reparation> AjouterReparation(Reparation reparation)
    {
        await Task.Run(() =>
        {
            lock (_reparationLock)
            {
                _context.Reparations.Add(reparation);
                _context.SaveChanges();
            }
        });
        return reparation;
    }

    public async Task SimulerReparationAsync(Ordinateur ordinateur, string technicien)
    {
        // Création object reparation (test)
        var test = new Reparation
        {
            UnOrdinateur = ordinateur,
            TypeReparation = "Réparation classique",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = technicien
        };

        //Méthode de création d'une réparation en base. (à créér par vous)
        var reparation = await AjouterReparation(test);

        await Task.Delay(2000);
        lock (_reparationLock)
        {
            reparation.Statut = "Terminée";
            _context.SaveChanges();
            _journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} réparé");
        }
        Console.WriteLine(
            $"{ordinateur.Nom} réparé");
    }

    public async Task<List<Reparation>> RecupererReparations()
    {
        return await _context.Reparations
            .Select(r => new Reparation
            {
                Id = r.Id,
                UnOrdinateur = r.UnOrdinateur,
                TypeReparation = r.TypeReparation,
                DateDebut = r.DateDebut,
                Statut = r.Statut,
                Technicien = r.Technicien
            })
            .ToListAsync();
    }

    public async Task<List<HistoriqueReparationRecord>> RecupererHistorique()
    {
        return await _context.Reparations
            .OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(
                r.UnOrdinateur.Id,
                r.UnOrdinateur.Nom,
                r.Technicien,
                r.TypeReparation,
                r.DateDebut))
            .ToListAsync();
    }
}
