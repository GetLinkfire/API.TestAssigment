namespace Service.Interfaces
{
    public interface IUniqueLinkService
    {
        string GetUniqueLinkShortCode(string domainName);

        bool IsValidLinkCode(string domainName, string code);
    }
}