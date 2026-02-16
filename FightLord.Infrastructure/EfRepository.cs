using System.Collections.Generic;
using System.Threading.Tasks;
using FightLord.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FightLord.Infrastructure
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        protected readonly FightLordDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EfRepository(FightLordDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

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
}
