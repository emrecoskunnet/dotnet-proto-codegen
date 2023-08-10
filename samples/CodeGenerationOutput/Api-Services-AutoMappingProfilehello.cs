namespace Foo.Core.Api.Services;

public class AutoMappingProfilehello : AutoMapper.Profile
{
	public AutoMappingProfilehello()
	{
		CreateMap<Foo.Core.Api.Grpc.CreateRequest, Foo.Core.Create>()
			.ConvertUsing( (createRequest, _, _) => new Foo.Core.Create(
				 // todo Complete ctor call
			){ Id = createRequest.CreateId ?? default });

		CreateMap<Foo.Core.Entry, Foo.Core.Api.Grpc.Entry>()
			.ForMember(i => i.EntryId, o => o.MapFrom(l => l.Id))
				 // todo Complete member mappings

		CreateMap<Foo.Core.Api.Grpc.ListQuery, SharedKernel.Querying.ListInputModel>();

		CreateMap<Foo.Core.Projections.ObjectListItemDto, Foo.Core.Api.Grpc.ObjectList.Types.ObjectListItem>();

		CreateMap<Foo.Core.Projections.ObjectLookupListItemDto, Foo.Core.Api.Grpc.ObjectLookupList.Types.ObjectLookupListItem>();

		// todo add DateTime mapping if not exists
		// CreateMap<DateTime, Google.Protobuf.WellKnownTypes.Timestamp>()
			// .ConvertUsing(l => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(l));
		// CreateMap<Google.Protobuf.WellKnownTypes.Timestamp, DateTime>()
			// .ConvertUsing(l => l.ToDateTime());
	}
}
