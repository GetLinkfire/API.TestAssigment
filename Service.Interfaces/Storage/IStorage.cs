using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces.Storage
{
	public interface IStorage
	{
        Task<string> GetAsync(string filePath);

        Task<T> GetAsync<T>(string filePath);

        Task SaveAsync(string filePath, string content);

        Task SaveAsync<T>(string filePath, T content);

		List<string> GetFileList(string directoryPath, string startedWith = null);

		void Delete(string directoryPath);
	}
}
