using AutoMapper;

namespace Service.Tests.Link
{
    internal static class AutoMapperConfig
    {
        public static void Configure()
        {
            Mapper.Reset();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<Mapping.MappingProfile>();
            });
        }
    }
}
