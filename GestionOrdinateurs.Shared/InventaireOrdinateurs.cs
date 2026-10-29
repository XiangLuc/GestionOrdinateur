namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    public Ordinateur? this[string reference]
    {
        get => Ordinateurs.FirstOrDefault(o => o.Nom == reference);
    }

    public Ordinateur? this[int index]
    {
        get => Ordinateurs.FirstOrDefault(o => o.Id == index);
    }
}
