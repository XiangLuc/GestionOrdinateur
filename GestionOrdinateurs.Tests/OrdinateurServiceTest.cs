using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Xunit;
using Microsoft.EntityFrameworkCore;

/*
On a ajouté FluentAssertion mais j'ai pas vu de question disant d'utiliser cette lib pour les tests unitaires donc
j'ai fais simple en gardant les tests existante et également pas vu d'exemple dans le cours
*/

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    // J'ai modifié la classe ParcInformatiqueContext car le code donné demander un paramètre "option" donc j'ai du faire un contructeur.
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    [Fact]
    public async Task AjouterOrdinateur_NouvelOrdinateur_EstDansLaBase()
    {
        var context = CreerContexte();
        var service = new OrdinateurService(context);
        var ordinateur = new Ordinateur { Id = 1, Nom = "PC-Test", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel") };

        await service.AjouterOrdinateur(ordinateur);

        Assert.Contains(context.Ordinateurs, o => o.Id == 1);
    }

    [Fact]
    public async Task SupprimerOrdinateurParId_IdExistant_RetourneVrai()
    {
        var context = CreerContexte();
        context.Ordinateurs.Add(new Ordinateur { Id = 2, Nom = "PC-Test", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel") });
        context.SaveChanges();
        var service = new OrdinateurService(context);

        bool resultat = await service.SupprimerOrdinateurParId(2);

        Assert.True(resultat);
        Assert.DoesNotContain(context.Ordinateurs, o => o.Id == 2);
    }

    [Fact]
    public async Task SupprimerOrdinateurParId_IdInexistant_RetourneFaux()
    {
        var context = CreerContexte();
        var service = new OrdinateurService(context);

        bool resultat = await service.SupprimerOrdinateurParId(-1);

        Assert.False(resultat);
    }

    [Fact]
    public async Task ChercherOrdinateurParMarque_MarqueDell_RetourneLesOrdinateursDell()
    {
        var context = CreerContexte();
        context.Ordinateurs.Add(new Ordinateur { Id = 3, Nom = "PC-Test", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel") });
        context.Ordinateurs.Add(new Ordinateur { Id = 4, Nom = "PC-Test", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Asus", "AMD") });
        context.Ordinateurs.Add(new Ordinateur { Id = 5, Nom = "PC-Test", Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable, "Dell", "Intel") });
        context.SaveChanges();
        var service = new OrdinateurService(context);

        List<Ordinateur> resultat = await service.ChercherOrdinateurParMarque("Dell");

        Assert.Equal(2, resultat.Count);
        Assert.All(resultat, o => Assert.Equal("Dell", o.Caracteristiques.Marque));
    }
}