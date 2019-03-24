using AutoMapper;

namespace WebApp.Mapping
{
	public static class AutoMapperConfig
	{
		public static void Configure()
		{
			Mapper.Initialize(cfg =>
			{
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<Service.Mapping.MappingProfile>();
            });
		}
	}
}