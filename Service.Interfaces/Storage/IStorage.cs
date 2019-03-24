using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces.Storage
{
	public interface IStorage
	{
        /// <summary>
        /// Get content of the file as a string
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>Content as string</returns>
        Task<string> GetAsync(string filePath);

        /// <summary>
        /// Get content of the file deserialized
        /// </summary>
        /// <typeparam name="T">Class to be used for deserialization</typeparam>
        /// <param name="filePath">Path to file</param>
        /// <returns>Content as instance of <typeparamref name="T"/></returns>
        Task<T> GetAsync<T>(string filePath);

        /// <summary>
        /// Save text content to a file
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="content">Text content</param>
        /// <returns></returns>
        Task SaveAsync(string filePath, string content);

        /// <summary>
        /// Save instance of a class to a file
        /// </summary>
        /// <typeparam name="T">Type used for serialization</typeparam>
        /// <param name="filePath">Path to file</param>
        /// <param name="content">Instance of <typeparamref name="T"/> to save</param>
        /// <returns></returns>
        Task SaveAsync<T>(string filePath, T content);

        /// <summary>
        /// Returns the list of files of directory
        /// </summary>
        /// <param name="directoryPath">Path to directory</param>
        /// <param name="startedWith">Start of filenames for search pattern</param>
        /// <returns></returns>
		List<string> GetFileList(string directoryPath, string startedWith = null);

        /// <summary>
        /// Delete a directory
        /// </summary>
        /// <param name="directoryPath">Path to directory</param>
		void Delete(string directoryPath);
	}
}
