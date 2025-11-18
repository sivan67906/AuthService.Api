using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Common.Interfaces;

public interface IQueryDbContext
{
    DbSet<T> Set<T>() where T : class;
}
