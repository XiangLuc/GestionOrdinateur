namespace GestionOrdinateurs.Client;

public record HistoriqueReparationDto(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention
);
