namespace Service.Helpers
{
    // Removed Storage-related thigs to another service since static helpers should do simple static things
    // And they do not expect some scoped services injected
    public static class LinkHelper
	{
		public static string GetShortLink(string domain, string code) => $"{domain}/{code}";

		public static string GetLinkGeneralFilename(string shortLink) => $"{shortLink}/general.json";
    }
}
