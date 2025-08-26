using FoxholeTrade.Data;
using FoxholeTrade.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FoxholeTrade.WebApp;

public static partial class ApplicationExtensions
{
    public static void ConfigureAppAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthorization();
    }
}
