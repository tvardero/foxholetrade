using FoxholeTrade.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace FoxholeTrade.Data.Seeding;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IHostEnvironment _env;
    private readonly RoleManager<Role> _roleManager;

    public DatabaseSeeder(AppDbContext db, IHostEnvironment env, RoleManager<Role> roleManager)
    {
        _db = db;
        _env = env;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRoles(cancellationToken);
    }

    private async Task SeedRoles(CancellationToken cancellationToken)
    {
        foreach (string roleName in Role.AllRoles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Role? role = await _roleManager.FindByNameAsync(roleName);

            if (role != null) continue;

            cancellationToken.ThrowIfCancellationRequested();
            await _roleManager.CreateAsync(new Role { Name = roleName });
        }
    }
}
