﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Common.Dto.Api;
using Core.Domain.Abstract.Repositories.Base;
using Core.Domain.Dto;
using Core.Domain.Entity.Base;
using Core.Infrastructure.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.DataAccess.Repositories.Base
{
    public abstract class BaseRepository<T, Type> : IBaseRepository<T, Type> where T : BaseEntity<Type>, new()
    {
        protected DbSet<T> _dbSet;
        private readonly IUnitOfWork _uow;
        public BaseRepository(IUnitOfWork uow)
        {
            _uow = uow;
            _dbSet = _uow.Set<T>();
        }

        public virtual async Task<Type> DeleteAsync(Type id)
        {
            var entity = await _dbSet.FindAsync(id);
            entity.IsRemoved = true;
            _uow.SaveChanges();
            return entity.Id;
        }

        public virtual async Task<SearchResult<T, BaseSearchParameter>> GetListAsync(BaseSearchParameter searchParameters)
        {
            var result = new SearchResult<T, BaseSearchParameter>
            {
                SearchParameter = searchParameters
            };
            var query = _dbSet.AsNoTracking().OrderByDescending(c => c.Id).AsQueryable();

            if (searchParameters.SearchParameter != default(DateTime))
            {
                query = query.Where(c => c.UpdateDate <= searchParameters.SearchParameter);
            }

            if (searchParameters.NeedTotalCount)
            {
                result.TotalCount = query.Count();
            }

            result.Result = await query.Take(searchParameters.PageSize).ToListAsync();
            return result;
        }

        public async Task<IEnumerable<T>> GetListAsync(PaginationDto pagination)
        {
            var query = _dbSet.AsNoTracking().OrderByDescending(c => c.Id).AsQueryable();
            var result = await query.Skip(pagination.PageIndex).Take(pagination.PageSize).ToListAsync();
            return result;
        }

        public virtual async Task<T> InsertAsync(T entity)
        {
            _dbSet.Add(entity);
            await _uow.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<Type> UpdateAsync(T entity)
        {
            var model = await FindAsync(entity.Id);
            if (model == null)
            {
                return default(Type); //equal null
            }
            _uow.Entry(model).CurrentValues.SetValues(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public virtual async Task<T> FindAsync(Type id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual T Find(Type id)
        {
            return _dbSet.Find(id);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        public virtual IQueryable<T> GetDbSet(Expression<Func<T, bool>> expression)
        {
            IQueryable<T> localEntities = _dbSet.AsQueryable();
            if (expression != null)
            {
                localEntities = localEntities.Where(expression);
            }
            return localEntities;
        }

        public virtual DbSet<T> GetDbSet()
        {
            return _dbSet;
        }

        public virtual async Task<SearchResult<T>> GetListAsync()
        {
            var result = new SearchResult<T>();
            var query = _dbSet.AsNoTracking().OrderByDescending(c => c.Id).AsQueryable();

            result.Result = await query.ToListAsync();
            return result;
        }
    }
}
