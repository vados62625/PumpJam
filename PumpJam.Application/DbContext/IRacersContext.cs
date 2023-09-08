using Microsoft.EntityFrameworkCore;

namespace PumpJam.Application.DbContext;

public interface IRacersContext
{
    DbSet<T> Set<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}