using System;
using System.Threading.Tasks;
using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IDomainRepository
	{
		Task<Domain> GetByIdAsync(Guid id);
	}
}
