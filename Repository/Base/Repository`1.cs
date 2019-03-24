using System;
using System.Threading.Tasks;
using Repository.Entities;
using Repository.Entities.Interfaces;
using Repository.Exceptions;

namespace Repository.Base
{
    public abstract class Repository<TEntity> : Repository<LinksContext, TEntity>
        where TEntity : class, IEntity<Guid>
    {
        public Repository(LinksContext context) : base(context)
        {
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            var entity = await FindAsync(id);
            if (entity == null)
            {
                throw new NotFoundException(typeof(TEntity), id);
            }

            return entity;
        }
    }
}
