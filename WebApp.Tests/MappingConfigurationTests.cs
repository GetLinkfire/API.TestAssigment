using AutoMapper;
using NUnit.Framework;
using WebApp.Mapping;

namespace WebApp.Tests
{
    [TestFixture]
    public class MappingConfigurationTests
    {
        [Test]
        public void AssertMappingConfiguration()
        {
            AutoMapperConfig.Configure();

            Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}
