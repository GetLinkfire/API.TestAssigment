using Autofac;
using Service.Interfaces;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Link;
using Service.Link.Arguments;
using Service.Models.Link;
using Service.Storage;

namespace Service
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // These could be Singletons since they are just wrappers around System.IO
            builder.RegisterType<StorageService>().As<IStorage>().SingleInstance();
            builder.RegisterType<UniqueLinkService>().As<IUniqueLinkService>().SingleInstance();

            // These could NEVER be Singletons since they are services that rely on Repositories which rely on Context
            // With SingleInstance you'll have one instance of Context per the whole life of your app (!!!!)
            // It will live in the root scope of Autofac and will never be disposed until the app dies
            // This is really important, I have no idea why you would ever put SingleInstance here
            builder.RegisterType<CreateLinkCommand>().As<ICommand<LinkModel, CreateLinkArgument>>();
            builder.RegisterType<UpdateLinkCommand>().As<ICommand<ExtendedLinkModel, UpdateLinkArgument>>();
            builder.RegisterType<GetLinkCommand>().As<ICommand<ExtendedLinkModel, GetLinkArgument>>();
            builder.RegisterType<DeleteLinkCommand>().As<ICommand<DeleteLinkArgument>>();
        }
    }
}
