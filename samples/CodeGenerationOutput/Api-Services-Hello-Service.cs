using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Foo.Core.Api.Services;

[Authorize]
// ReSharper disable once ClassNeverInstantiated.Global
public class HelloV1Service : Foo.Core.Api.Grpc.HelloService.HelloServiceBase
{
	private readonly IAppLocalizer _localizer;
	private readonly IMapper _mapper;
	private readonly IRepository<Foo.Core.Hello> _repository;

	public HelloV1Service(IAppLocalizer localizer, IMapper mapper, IRepository<Foo.Core.Hello> repository)
	{
		_localizer = localizer;
		_mapper = mapper;
		_repository = repository;
	}


	public override async Task<Foo.Core.Api.Grpc.ObjectList> GetList(Foo.Core.Api.Grpc.ListQuery request, ServerCallContext context)
	{
		try
		{
			ListInputModel? inputModel = _mapper.Map<Foo.Core.Api.Grpc.ListQuery, ListInputModel>(request);
			var pagedList = await _repository.PagedListAsync(
				request,
				inputModel,
				context.CancellationToken);

			Foo.Core.Api.Grpc.ObjectList result = new()
			{
				Total = pagedList.Total
			};
			result.Items.AddRange(pagedList.Select(i => _mapper.Map<Foo.Core.Api.Grpc.ObjectList.Types.ObjectListItem>(i)));
			return result;
		}
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}

	public override async Task<Foo.Core.Api.Grpc.ObjectLookupList> GetLookup(Foo.Core.Api.Grpc.Empty request, ServerCallContext context)
	{
		try
		{
			var list = await _repository.ListAsync(
				request,
				context.CancellationToken);

			ObjectLookupList result = new()
			{
				Total = list.Count
			};
			result.Items.AddRange(list.Select(i => _mapper.Map<Foo.Core.Api.Grpc.ObjectLookupList.Types.ObjectLookupListItem>(i)));
			return result;
		}
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}

	public override async Task<Foo.Core.Api.Grpc.Entry> GetById(Foo.Core.Api.Grpc.GetByIdRequest request, ServerCallContext context)
	{
		try
		{
			Foo.Core.Entry exists = await _repository.SingleAsync(
				request,
				context.CancellationToken);
			return _mapper.Map<Foo.Core.Api.Grpc.Entry>(exists);
		}
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}

	public override async Task<Foo.Core.Api.Grpc.Entry> Create(Foo.Core.Api.Grpc.CreateRequest request,
		ServerCallContext context)
	{
		try
		{
			Foo.Core.Entry entry =
				await _repository.AddAsync(_mapper.Map<Foo.Core.Entry>(request),
				context.CancellationToken);

			return _mapper.Map<Foo.Core.Api.Grpc.Entry>(entry);
		}
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (ArgumentException e) { throw new RpcException(new Status(StatusCode.InvalidArgument, localizer["The provided request is invalid"])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}

	public override async Task<Foo.Core.Api.Grpc.Entry> Update(Foo.Core.Api.Grpc.UpdateRequest request,
		ServerCallContext context)
	{
		try
		{
			Foo.Core.Entry exists = await _repository.SingleAsync(new EntrySpec(request.EntryId ?? 0, EntryFilter.Default));
			exists.Update(); // todo complete update code
			await _repository.SaveChangesAsync(context.CancellationToken);

			return _mapper.Map<Foo.Core.Api.Grpc.Entry>(exists);
		}
		catch (DomainObjectAlreadyExistsException e) { throw new RpcException(new Status(StatusCode.AlreadyExists, localizer["'0' already exists", request.GetType().Name])); }
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (ArgumentException e) { throw new RpcException(new Status(StatusCode.InvalidArgument, localizer["The provided request is invalid"])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}

	public override async Task<Foo.Core.Api.Grpc.Empty> Delete(Foo.Core.Api.Grpc.GetByIdRequest request,
		ServerCallContext context)
	{
		try
		{
			Foo.Core.Hello exists = await _repository.SingleAsync(new HelloSpec(request.HelloId, HelloFilter.All));
			exists.SoftDelete(); // todo complete delete code
			await _repository.SaveChangesAsync(context.CancellationToken);

			return new Empty();
		}
		catch (DomainObjectNotFoundException e) { throw new RpcException(new Status(StatusCode.NotFound, _localizer["'0' not found", request.GetType().Name])); }
		catch (ArgumentException e) { throw new RpcException(new Status(StatusCode.InvalidArgument, localizer["The provided request is invalid"])); }
		catch (Exception e){ throw new RpcException(new Status(StatusCode.Unknown, localizer["An error occurred while processing the request"])); }
	}
}
