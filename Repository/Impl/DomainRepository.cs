using Repository.Entities;
using Repository.Interfaces;

namespace Repository
{
    internal sealed class DomainRepository : Base.Repository<Domain>, IDomainRepository
	{
        public DomainRepository(LinksContext context) : base(context)
        {
        }
	}
}
