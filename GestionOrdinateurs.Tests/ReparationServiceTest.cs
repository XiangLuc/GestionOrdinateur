using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class ReparationServiceTest
{
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    [Fact]
    public async Task AjouterReparation_NouvelleReparation_EstDansLaBase()
    {
        var context = CreerContexte();
        var service = new ReparationService(context, new JournalService());
        var ordinateur = new Ordinateur { Id = 1, Nom = "Dell", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel") };
        var reparation = new Reparation
        {
            UnOrdinateur = ordinateur,
            TypeReparation = "Changement écran",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = "Test"
        };

        var resultat = await service.AjouterReparation(reparation);

        Assert.Contains(context.Reparations, r => r.Id == resultat.Id);
        Assert.Equal("Test", resultat.Technicien);
        Assert.Equal(1, resultat.UnOrdinateur.Id);
    }
}