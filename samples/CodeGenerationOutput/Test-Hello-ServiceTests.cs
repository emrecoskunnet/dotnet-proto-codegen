using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Foo.Core.ApiFunctionTests;

public class HelloServiceTests : IClassFixture<GrpcTestFixture<FooTestStartup, FooDbContext>>
{
	private readonly GrpcTestContext<FooTestStartup, FooDbContext> _testContext;

	public HelloServiceTests(GrpcTestFixture<FooTestStartup, FooDbContext> fixture)
	{
		_testContext = new GrpcTestContext<FooTestStartup, FooDbContext>(fixture);
		// todo add seed test data
	}

	[Fact]
	public async Task TestGetList_ReturnsListWithOneResult_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		// clear data and add one record
		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};

		Foo.Core.Api.Grpc.Entry? entry = await client.CreateAsync(createRequest);

		Foo.Core.Api.Grpc.ListQuery listQuery = new()
		{
		// todo complete request data
		};

		// Act
		Foo.Core.Api.Grpc.ObjectList? objectList = await client.GetListAsync(listQuery);

		// Assert
		Assert.Single(objectList.Items);
		Assert.Equal(entry.HelloId, objectList.Items.First().HelloId);
		// todo add list column asserts
	}

	[Fact]
	public async Task TestGetList_WhenNotFound_ShouldFail()
	{
		// Arrange
		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);
		Foo.Core.Api.Grpc.ListQuery listQuery = new()
		{
		// todo complete request data
		};

		// Act
		RpcException exception = await Assert.ThrowsAsync<RpcException>(async () => await client.GetListAsync(listQuery));

		// Assert
		Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
		Assert.Equal("'Hello' not found", exception.Status.Detail);
	}

	[Fact]
	public async Task TestGetLookup_ReturnsListWithOneResult_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		// clear data and add one record
		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};

		Foo.Core.Api.Grpc.Entry? entry = await client.CreateAsync(createRequest);

		Empty empty = new();

		// Act
		Foo.Core.Api.Grpc.ObjectLookupList? objectLookupList = await client.GetLookupAsync(empty);

		// Assert
		Assert.Single(objectLookupList.Items);
		Assert.Equal(entry.HelloId, objectLookupList.Items.First().HelloId);
		// todo add list column asserts
	}

	[Fact]
	public async Task TestGetLookup_WhenNotFound_ShouldFail()
	{
		// Arrange
		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);
		Empty empty = new();

		// Act
		RpcException exception = await Assert.ThrowsAsync<RpcException>(async () => await client.GetLookupAsync(empty));

		// Assert
		Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
		Assert.Equal("'Hello' not found", exception.Status.Detail);
	}

	[Fact]
	public async Task TestGetById_ReturnsListWithOneResult_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		// clear data and add one record
		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};

		Foo.Core.Api.Grpc.Entry? entry = await client.CreateAsync(createRequest);

		Foo.Core.Api.Grpc.GetByIdRequest getByIdRequest = new()
		{
		// todo complete request data
		};

		// Act
		Foo.Core.Api.Grpc.Entry? entry = await client.GetByIdAsync(getByIdRequest);

		// Assert
		Assert.Single(entry.Items);
		Assert.Equal(entry.HelloId, entry.Items.First().HelloId);
		// todo add list column asserts
	}

	[Fact]
	public async Task TestGetById_WhenNotFound_ShouldFail()
	{
		// Arrange
		var list = _testContext.DbContextAccessor.Hellos.ToList();
		_testContext.DbContextAccessor.Hellos.RemoveRange(list);
		await _testContext.DbContextAccessor.SaveChangesAsync();
		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);
		Foo.Core.Api.Grpc.GetByIdRequest getByIdRequest = new()
		{
		// todo complete request data
		};

		// Act
		RpcException exception = await Assert.ThrowsAsync<RpcException>(async () => await client.GetByIdAsync(getByIdRequest));

		// Assert
		Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
		Assert.Equal("'Hello' not found", exception.Status.Detail);
	}

	[Fact]
	public async Task TestCreate_WithValidRequest_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		Foo.Core.Api.Grpc.Entry? entry = await client.CreateAsync(createRequest);

		// Assert
		Assert.NotNull(entry);
		// todo add response asserts
	}

	[Fact]
	public async Task TestCreate_WithExistingName_ShouldFail()
	{
		// Arrange
		_testContext.DbContextAccessor.Hellos.Add(new Foo.Core.Hello( // todo complete test case data
			));
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};
		// Act

		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.CreateAsync(createRequest));

		// Assert
		Assert.Equal(StatusCode.AlreadyExists, exception.Status.StatusCode);
		Assert.Equal("'TODO COMPLETE NAME' already exists", exception.Status.Detail);
		Assert.True(exception.Trailers.Get(nameof(Foo.Core.Api.Grpc.CreateRequest.HelloName)) != null);
		Assert.Equal("TODO COMPLETE NAME",
			exception.Trailers.GetValue(nameof(Foo.Core.Api.Grpc.CreateRequest.HelloName)));
	}

	[Fact]
	public async Task TestCreate_WithInvalidRequest_ShouldFail()
	{
		// Arrange
		Foo.Core.Api.Grpc.CreateRequest createRequest = new()
		{
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.CreateAsync(createRequest));

		// Assert
		Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
		Assert.Equal("The provided request is invalid", exception.Status.Detail);
	}

	[Fact]
	public async Task TestUpdate_WithValidRequest_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Hello exists = new(){ 
			// todo complete test case data
			};
		 _testContext.DbContextAccessor.Hellos.Add(exists);
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.UpdateRequest updateRequest = new()
		{
			HelloId = exists.Id,
			// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		Foo.Core.Api.Grpc.Entry? entry = await client.UpdateAsync(updateRequest);

		// Assert
		Assert.NotNull(entry);
		// todo add response asserts
	}

	[Fact]
	public async Task TestUpdate_WithNonExistingId_ShouldFail()
	{
		// Arrange
		Foo.Core.Api.Grpc.UpdateRequest updateRequest = new()
		{
			HelloId = 9999,
			HelloName = "Non Exist Test Hello",
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.UpdateAsync(updateRequest));

		// Assert
		Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
		Assert.Equal("'9999' not found", exception.Status.Detail);
		Assert.Equal("9999", exception.Trailers.GetValue("criteria"));
	}

	[Fact]
	public async Task TestUpdate_WithAlreadyExistingName_ShouldFail()
	{
		// Arrange
		Foo.Core.Hello exists1 = new(){ 
			// todo complete test case data
			};
		Foo.Core.Hello exists2 = new(){ 
			// todo complete test case data
			};
		 _testContext.DbContextAccessor.Hellos.Add(exists1);
		 _testContext.DbContextAccessor.Hellos.Add(exists2);
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.UpdateRequest updateRequest = new()
		{
			HelloId = exists2.Id,
			HelloName = exists1.HelloName,
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.UpdateAsync(updateRequest));

		// Assert
		Assert.Equal(StatusCode.AlreadyExists, exception.Status.StatusCode);
		Assert.Equal($"'{exists1.HelloName.ToUpper()}' already exists", exception.Status.Detail);
		Assert.NotNull(exception.Trailers.Get(nameof(updateRequest.HelloName)));
		Assert.Equal(exists1.HelloName.ToUpper(),exception.Trailers.GetValue(nameof(Foo.Core.Api.Grpc.UpdateRequest.HelloName)));
	}

	[Fact]
	public async Task TestUpdate_WithInvalidArgumentRequest_ShouldFail()
	{
		// Arrange
		Foo.Core.Hello exists = new(){ 
			// todo complete test case data
			};
		 _testContext.DbContextAccessor.Hellos.Add(exists);
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.UpdateRequest updateRequest = new()
		{
			HelloId = exists.Id,
			HelloName = "  ",
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.UpdateAsync(updateRequest));

		// Assert
		Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
		Assert.Equal("The provided request is invalid", exception.Status.Detail);
	}

	[Fact]
	public async Task TestDelete_WithValidRequest_ShouldSucceed()
	{
		// Arrange
		Foo.Core.Hello exists = new(){ 
			// todo complete test case data
			};
		 _testContext.DbContextAccessor.Hellos.Add(exists);
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.GetByIdRequest getByIdRequest = new()
		{
			HelloId = exists.Id,
			// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		Empty empty = await client.DeleteAsync(getByIdRequest);

		// Assert
		Assert.NotNull(empty);
		// todo add response asserts
	}

	[Fact]
	public async Task TestDelete_WithNonExistingId_ShouldFail()
	{
		// Arrange
		Foo.Core.Api.Grpc.GetByIdRequest getByIdRequest = new()
		{
			HelloId = 9999,
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.DeleteAsync(getByIdRequest));

		// Assert
		Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
		Assert.Equal("'9999' not found", exception.Status.Detail);
		Assert.Equal("9999", exception.Trailers.GetValue("criteria"));
	}

	[Fact]
	public async Task TestDelete_WithInvalidArgumentRequest_ShouldFail()
	{
		// Arrange
		Foo.Core.Hello exists = new(){ 
			// todo complete test case data
			};
		 _testContext.DbContextAccessor.Hellos.Add(exists);
		await _testContext.DbContextAccessor.SaveChangesAsync();

		Foo.Core.Api.Grpc.GetByIdRequest getByIdRequest = new()
		{
			HelloId = exists.Id
		// todo complete request data
		};

		Foo.Core.Api.Grpc.HelloService.HelloServiceClient client = new(_testContext.Channel);
		await client.DeleteAsync(getByIdRequest);

		// Act
		RpcException exception =
			await Assert.ThrowsAsync<RpcException>(async () => await client.DeleteAsync(getByIdRequest));

		// Assert
		Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
		Assert.Equal("The provided request is invalid", exception.Status.Detail);
	}

}
