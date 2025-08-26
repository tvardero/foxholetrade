using FoxholeTrade.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoxholeTrade.Data;

/// <summary>
/// EntityFrameworkCore database context for FoxholeTrade.
/// </summary>
public class AppDbContext : IdentityDbContext<User, Role, Guid>
{
    /// <inheritdoc />
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
