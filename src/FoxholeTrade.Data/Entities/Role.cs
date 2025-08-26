using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FoxholeTrade.Data.Entities;

public class Role : IdentityRole<Guid>
{
    [NotMapped]
    public static string[] AllRoles { get; } = [SUPER_ADMIN];

    public const string SUPER_ADMIN = "SuperAdmin";
};
