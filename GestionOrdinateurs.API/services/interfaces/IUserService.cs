using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IUserService
{
    Task<User?> ChercherUtilisateurParLogin(string login);
    Task<List<User>> RecupererUtilisateurs();
}
