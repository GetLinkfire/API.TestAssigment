namespace Service.Interfaces
{
    public interface IUniqueLinkService
    {
        /// <summary>
		/// Generate an unique short code for link
		/// </summary>
		/// <param name="storageService"></param>
		/// <param name="domainName"></param>
		/// <returns></returns>
        string GetUniqueLinkShortCode(string domainName);

        /// <summary>
        /// Checks if the link code is valid
        /// </summary>
        /// <param name="storageService"></param>
        /// <param name="domainName"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        bool IsValidLinkCode(string domainName, string code);
    }
}