using System.Linq.Expressions;
using Domain.Models;
using PumpJam.Application.DbContext;
using PumpJam.Application.Repository;
using PumpJam.DB;

namespace PumpJam.Repository;

public class RacersRepository : RepositoryBase<RacerDB, int, RacersContext>
{
    private readonly IRacersContext _dbContext;
    protected override Expression<Func<RacerDB, int>> Key => model => model.Id;
    public RacersRepository(RacersContext context)
        : base(context, ctx => ctx.Set<RacerDB>())
    {
    }
}