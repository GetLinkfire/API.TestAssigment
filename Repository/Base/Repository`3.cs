using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Repository.Entities.Interfaces;

namespace Repository.Base
{
    public abstract class Repository<TContext, TEntity, TKey>
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
    {
        protected TContext Context { get; }

        public Repository(TContext context)
        {
            Context = context;
        }

        protected Task<TEntity> FindAsync(TKey id) => Context.Set<TEntity>().FindAsync(id);

        protected Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) => Context.Set<TEntity>().Where(predicate).ToListAsync();
    }
}
