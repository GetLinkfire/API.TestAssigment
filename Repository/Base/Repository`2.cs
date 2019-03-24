using System;
using System.Data.Entity;
using Repository.Entities.Interfaces;

namespace Repository.Base
{
    public abstract class Repository<TContext, TEntity> : Repository<TContext, TEntity, Guid>
        where TContext : DbContext
        where TEntity : class, IEntity<Guid>
    {
        public Repository(TContext context) : base(context)
        {
        }
    }
}
