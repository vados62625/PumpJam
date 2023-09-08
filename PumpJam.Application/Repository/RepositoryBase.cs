using System.Linq.Expressions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace PumpJam.Application.Repository;

public abstract class RepositoryBase<TModel, TKey, TDbContext> where TModel : class
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    protected readonly DbSet<TModel> _dBSet;
    private readonly TDbContext _dbContext;

    protected abstract Expression<Func<TModel, TKey>> Key { get; }

    protected RepositoryBase(TDbContext dbContext, Func<TDbContext, DbSet<TModel>> dBSet)
    {
        _dbContext = dbContext;
        _dBSet = dBSet(dbContext);
    }

    public virtual async Task<TModel[]> GetAll()
    {
        return await _dBSet.ToArrayAsync();
    }

    public async Task<TModel[]> OrderBy(Expression<Func<TModel, TKey>> expression)
    {
        return await _dBSet.OrderBy(expression).ToArrayAsync();
    }

    public async Task<TModel[]> OrderByDescending(Expression<Func<TModel, TKey>> expression)
    {
        return await _dBSet.OrderByDescending(expression).ToArrayAsync();
    }

    public async Task<TModel[]> Paginate(int offset, int limit)
    {
        return await _dBSet.OrderBy(Key).Skip(offset).Take(limit).ToArrayAsync();
    }

    public virtual async Task<TModel[]> Get(Expression<Func<TModel, bool>> predicate)
    {
        return await _dBSet.Where(predicate).ToArrayAsync();
    }

    public virtual async Task<TModel?> First(Expression<Func<TModel, bool>> predicate)
    {
        return await _dBSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<TModel> Add(TModel model)
    {
        try
        {
            await _dBSet.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return model;
        }
        catch (DbUpdateException exception)
        {
            return null;
        }
    }

    public async Task AddRange(IEnumerable<TModel> range)
    {
        try
        {
            await _dBSet.AddRangeAsync(range);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
        }
    }

    public async Task Remove(TModel model)
    {
        try
        {
            _dbContext.Entry<TModel>(model).State = EntityState.Deleted;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            return;
        }
    }

    public async Task<TModel> Update(TModel entity)
    {
        try
        {
            _dBSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await exception.Entries.Single().ReloadAsync();
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return entity;
    }

    public async Task<TModel[]> Where(Expression<Func<TModel, bool>> predicate)
    {
        return await _dBSet.Where(predicate).ToArrayAsync();
    }

    public async Task<long> Count()
    {
        return await _dBSet.LongCountAsync();
    }
}