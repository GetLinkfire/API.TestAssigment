using Autofac;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // EF's DbContext is supposed to be a small short-living object
            // So it's advised to always register Context as InstancePerDependency()  
            // As well as for Repositories which basically should be 'bags' for db objects
            // However according to the current implementaion of the services InstancePerLifetimeScope() would also work
            // Since the lifetime scope of repositories will be the scope of services which (now) per Dependency
            builder.RegisterType<LinksContext>().InstancePerLifetimeScope();

            builder.RegisterType<LinkRepository>().As<ILinkRepository>().InstancePerLifetimeScope();
            builder.RegisterType<DomainRepository>().As<IDomainRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MediaServiceRepository>().As<IMediaServiceRepository>().InstancePerLifetimeScope();
        }
    }
}
