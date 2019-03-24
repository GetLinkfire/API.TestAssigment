using System;
using System.Threading.Tasks;
using Repository.Entities;

namespace Repository.Interfaces
{
	public interface ILinkRepository
	{
		/// <summary>
		/// Get active link with domain by id
		/// <exception cref="NotFoundException">when link does not exists in db.</exception>
		/// </summary>
		/// <param name="linkId"></param>
		/// <returns></returns>
		Task<Link> GetByIdAsync(Guid linkId);

		/// <summary>
		/// Create link
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		Task<Link> CreateAsync(Link link);

		/// <summary>
		/// Update existing link
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		Task<Link> UpdateAsync(Link link);

        /// <summary>
        /// Delete link logicaly (set IsActive to false)
        /// <exception cref="NotFoundException">when link does not exists in db.</exception>
        /// <exception cref="IllegalArgumentException">when link is not active already.</exception>
        /// </summary>
        /// <param name="id"></param>
        Task<Link> DeleteAsync(Guid id);
	}
}
