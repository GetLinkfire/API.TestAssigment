using System;
using System.IO;
using System.Linq;
using Service.Interfaces;
using Service.Interfaces.Storage;

namespace Service.Link
{
    public class UniqueLinkService : IUniqueLinkService
    {
        private readonly IStorage storageService;

        public UniqueLinkService(IStorage storageService)
        {
            this.storageService = storageService;
        }
        
		public string GetUniqueLinkShortCode(string domainName)
        {
            var code = string.Empty;
            do
            {
                var randomString = GetRandomString();
                code = randomString.Substring(0, randomString.IndexOf('-'));
            } while (!IsValidLinkCode(domainName, code));

            return code;
        }

		public bool IsValidLinkCode(string domainName, string code)
        {
            var shorterCode = code.Substring(0, code.Length - 2);

            var keysInPath = storageService.GetFileList(domainName, shorterCode);

            foreach (string key in keysInPath)
            {
                var existingCode = key.Split(Path.DirectorySeparatorChar).Last();

                // check code from key against short and shorter code
                if (AreStringsEqual(existingCode, code) || AreStringsEqual(existingCode, shorterCode))
                {
                    return false;
                }

                // check shorter code from key against the requested short code
                if (AreStringsEqual(existingCode.Substring(0, existingCode.Length - 2), code))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetRandomString() => Guid.NewGuid().ToString();

        private static bool AreStringsEqual(string string1, string string2) => string1.Equals(string2, StringComparison.InvariantCultureIgnoreCase);
    }
}
