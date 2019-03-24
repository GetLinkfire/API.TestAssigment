using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Service.Interfaces.Storage;

namespace Service.Storage
{
	public class StorageService : IStorage
	{
		private string _tempFolder = "data";

		private static string SolutionFolder
		{
			get
			{
                // I want to cry then looking at this, why simple System.IO.Path.GetTempPath() wouldn't work?
                // Plus what if I'd like to move build artefacts to other folder containing 'bin' (e.g. './bin-build')?
                // Then the data folder will be place under Parent-parent-parent which may not even exist
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
				if (basePath.Contains("bin"))
					return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
				return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName;
			}
		}

        public async Task<string> GetAsync(string filePath)
        {
            var absolutePath = Path.Combine(SolutionFolder, _tempFolder, filePath);
            if (!File.Exists(absolutePath))
            {
                throw new Exception($"File {absolutePath} not found.");
            }

            using (var reader = new StreamReader(absolutePath))
            {
                return await reader.ReadToEndAsync();
            }
        }


        public async Task<T> GetAsync<T>(string filePath)
		{
            var content = await GetAsync(filePath);
            return JsonConvert.DeserializeObject<T>(content);
		}

        public async Task SaveAsync(string filePath, string content)
        {
            var absolutePath = Path.Combine(SolutionFolder, _tempFolder, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            using (var writer = new StreamWriter(absolutePath))
            {
                await writer.WriteAsync(content);
            }
        }

        public async Task SaveAsync<T>(string filePath, T content)
		{
			var text = JsonConvert.SerializeObject(content);
            await SaveAsync(filePath, text);
        }

		public List<string> GetFileList(string directoryPath, string startedWith = null)
		{
			var absolutePath = Path.Combine(SolutionFolder, _tempFolder, directoryPath);
			if (!Directory.Exists(absolutePath))
			{
				return Enumerable.Empty<string>().ToList();
			}

			var attr = File.GetAttributes(absolutePath);
			if (!attr.HasFlag(FileAttributes.Directory))
			{
				throw new ArgumentException($"Path {directoryPath} should point to directory");
			}

			var filePaths = Directory.GetFileSystemEntries(absolutePath,
				string.IsNullOrEmpty(startedWith) ? $"*.json" : $"{startedWith}*", SearchOption.AllDirectories);

			return filePaths.ToList();
		}

		public void Delete(string directoryPath)
		{
			var absolutePath = Path.Combine(SolutionFolder, _tempFolder, directoryPath);
			var attr = File.GetAttributes(absolutePath);
			if (!attr.HasFlag(FileAttributes.Directory))
			{
				throw new ArgumentException($"Path {directoryPath} should point to directory");
			}
			
			Directory.Delete(absolutePath, true);
		}
	}
}
