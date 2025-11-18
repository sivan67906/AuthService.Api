using Microsoft.AspNetCore.Identity;
using System;

namespace AuthService.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
