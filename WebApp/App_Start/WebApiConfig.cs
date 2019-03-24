using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json.Converters;
using WebApp.Mapping;

namespace WebApp
{
    public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			AutoMapperConfig.Configure();
            
            AutofacConfig.Configure(config);

            config.MapHttpAttributeRoutes();

            SetupFormatters(config);

            config.EnsureInitialized();
		}

        private static void SetupFormatters(HttpConfiguration config)
        {
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            config.Formatters.JsonFormatter.Indent = true;
        }
	}
}
