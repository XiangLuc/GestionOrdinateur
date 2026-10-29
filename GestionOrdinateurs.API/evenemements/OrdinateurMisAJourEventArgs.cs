using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public class OrdinateurMisAJourEventArgs: EventArgs
{
    public int Id { get; set; }
    public string Nom { get; set; }
}
