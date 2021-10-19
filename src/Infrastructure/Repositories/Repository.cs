using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Common.Interfaces.Infrastructure.Repositories;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        private readonly ApplicationContext _context;
        public IUnitOfWork UnitOfWork => _context;

        private readonly DbSet<TEntity> _dbSet;

        public Repository(ApplicationContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }
        
        public TEntity Add(TEntity entity)
        {
            return entity.IsTransient() ? _dbSet.Add(entity).Entity : entity;
        }
        
        public TEntity Update(TEntity entity)
        {
            return !entity.IsTransient() ? _dbSet.Update(entity).Entity : entity;
        }
        
        public bool Remove(TEntity entity)
        {
            if (entity.IsTransient()) return false;
            
            _dbSet.Remove(entity);
            return true;
        }

        public async Task<bool> Any(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<TEntity> FindById(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync();
        }
        
        public async Task<IEnumerable<TEntity>> FindAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> FindOneBy(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
    }
}