using BicycleShop.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BicycleShop.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly BicycleShopDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(BicycleShopDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}