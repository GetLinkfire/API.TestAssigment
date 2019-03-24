using AutoMapper;
using Repository.Entities;
using Service.Models;
using Service.Models.Link;

namespace Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Models.Enums.MediaType, Repository.Entities.Enums.MediaType>()
                .ReverseMap();

            CreateMap<ArtistModel, Artist>()
                .ForMember(dest => dest.Id, exp => exp.UseDestinationValue())
                .ForMember(dest => dest.Name, exp => exp.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Label, exp => exp.MapFrom(src => src.Label.Trim()));

            CreateMap<LinkModel, Repository.Entities.Link>()
                .ForMember(dest => dest.RowVersion, exp => exp.Ignore())
                .ForMember(dest => dest.UpdatedAt, exp => exp.Ignore())
                .ForMember(dest => dest.Domain, exp => exp.Ignore())
                .ForMember(dest => dest.IsActive, exp => exp.Ignore())
                .ForMember(dest => dest.MediaType, exp => exp.Ignore());

            CreateMap<Repository.Entities.Link, LinkModel>();

            CreateMap<Repository.Entities.Link, ExtendedLinkModel>()
                .IncludeBase<Repository.Entities.Link, LinkModel>()
                .ReverseMap();

            CreateMap<LinkModel, Models.StorageModel.Base.StorageModel>();
            CreateMap<LinkModel, Models.StorageModel.Music.StorageModel>()
                .IncludeBase<LinkModel, Models.StorageModel.Base.StorageModel>();

            CreateMap<ExtendedLinkModel, Models.StorageModel.Music.StorageModel>()
                .IncludeBase<LinkModel, Models.StorageModel.Base.StorageModel>()
                .ForMember(dest => dest.TrackingInfo, exp => exp.Ignore())
                .ForMember(dest => dest.Destinations, exp => exp.Ignore());

            CreateMap<LinkModel, Models.StorageModel.Ticket.StorageModel>()
                .IncludeBase<LinkModel, Models.StorageModel.Base.StorageModel>()
                .ForMember(dest => dest.Destinations, exp => exp.Ignore());

            CreateMap<ExtendedLinkModel, Models.StorageModel.Ticket.StorageModel>()
                .IncludeBase<LinkModel, Models.StorageModel.Base.StorageModel>();

            CreateMap<Models.Link.Music.TrackingModel, Models.StorageModel.Music.TrackingStorageModel>()
                 .ForMember(dest => dest.MediaServiceName, exp => exp.Ignore())
                 .ReverseMap();

            CreateMap<Models.Link.Base.DestinationModel, Models.StorageModel.Base.DestinationStorageModel>()
                .ReverseMap();

            CreateMap<Models.Link.Music.DestinationModel, Models.StorageModel.Music.DestinationStorageModel>()
                .IncludeBase<Models.Link.Base.DestinationModel, Models.StorageModel.Base.DestinationStorageModel>()
                .ReverseMap();

            CreateMap<Models.Link.Ticket.DestinationModel, Models.StorageModel.Ticket.DestinationStorageModel>()
                .IncludeBase<Models.Link.Base.DestinationModel, Models.StorageModel.Base.DestinationStorageModel>()
                .ReverseMap();
        }
    }
}
