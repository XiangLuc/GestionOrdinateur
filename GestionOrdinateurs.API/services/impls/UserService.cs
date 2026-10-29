using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class UserService : IUserService
{
    private readonly ParcInformatiqueContext _context;

    public UserService(ParcInformatiqueContext context)
    {
        _context = context;
    }

    public async Task<User?> ChercherUtilisateurParLogin(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<List<User>> RecupererUtilisateurs()
    {
        return await _context.Users.ToListAsync();
    }
}
