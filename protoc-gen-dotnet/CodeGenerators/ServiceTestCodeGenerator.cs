using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class ServiceTestCodeGenerator: ICodeGenerationStrategy
{
    public string Name => "test";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        return from serviceType in protoFiles.SelectMany((x) => x.Service) let entityName = serviceType.Name.Replace("Service", "") select ServiceTestCodeGeneration(options, entityName, serviceType);
    }

    private static CodeGeneratorResponse.Types.File ServiceTestCodeGeneration(CodeGenerationStrategyOptions options, string entityName,
        ServiceDescriptorProto serviceType)
    {
        StringBuilder outputServiceTest = new();
        outputServiceTest.AppendLine("using Google.Protobuf.WellKnownTypes;");
        outputServiceTest.AppendLine("using Grpc.Core;");
        outputServiceTest.AppendLine();
        outputServiceTest.AppendLine($"namespace {options.CoreNamespace}.ApiFunctionTests;");
        outputServiceTest.AppendLine();
        outputServiceTest.AppendLine(
            $"public class {serviceType.Name}Tests : IClassFixture<GrpcTestFixture<{options.BaseName}TestStartup, {options.BaseName}DbContext>>");
        outputServiceTest.AppendLine("{");
        outputServiceTest.AppendLine(
            @$"{"\t"}private readonly GrpcTestContext<{options.BaseName}TestStartup, {options.BaseName}DbContext> _testContext;");
        outputServiceTest.AppendLine();
        outputServiceTest.AppendLine(
            @$"{"\t"}public {serviceType.Name}Tests(GrpcTestFixture<{options.BaseName}TestStartup, {options.BaseName}DbContext> fixture)");
        outputServiceTest.AppendLine(@$"{"\t"}{{");
        outputServiceTest.AppendLine(
            @$"{"\t\t"}_testContext = new GrpcTestContext<{options.BaseName}TestStartup, {options.BaseName}DbContext>(fixture);");
        outputServiceTest.AppendLine(@$"{"\t\t"}// todo add seed test data");
        outputServiceTest.AppendLine(@$"{"\t"}}}");
        outputServiceTest.AppendLine();

        foreach (MethodDescriptorProto? method in serviceType.Method)
        {
            string requestType = method.InputType.Split(".").LastOrDefault() ?? string.Empty;
            string responseType = method.OutputType.Split(".").LastOrDefault() ?? string.Empty;

            if (method.Name.Contains("Create") || method.Name.Contains("Add"))
            {
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithValidRequest_ShouldSucceed()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{responseType}? {responseType.ConvertToCamelCase()} = await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.NotNull({responseType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo add response asserts");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(@$"{"\t"}public async Task Test{method.Name}_WithExistingName_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}_testContext.DbContextAccessor.{entityName}s.Add(new {options.CoreNamespace}.{entityName}( // todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}));");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.AlreadyExists, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""'TODO COMPLETE NAME' already exists"", exception.Status.Detail);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.True(exception.Trailers.Get(nameof({options.ApiGrpcNamespace}.{requestType}.{entityName}Name)) != null);");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(""TODO COMPLETE NAME"",");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}exception.Trailers.GetValue(nameof({options.ApiGrpcNamespace}.{requestType}.{entityName}Name)));");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();


                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(@$"{"\t"}public async Task Test{method.Name}_WithInvalidRequest_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""The provided request is invalid"", exception.Status.Detail);");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
            }
            else if (method.Name.Contains("Update") || method.Name.Contains("Edit"))
            {
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithValidRequest_ShouldSucceed()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{responseType}? {responseType.ConvertToCamelCase()} = await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.NotNull({responseType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo add response asserts");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(@$"{"\t"}public async Task Test{method.Name}_WithNonExistingId_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = 9999,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Name = ""Non Exist Test {entityName}"",");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(""'9999' not found"", exception.Status.Detail);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""9999"", exception.Trailers.GetValue(""criteria""));");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithAlreadyExistingName_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists1 = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists2 = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists1);");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists2);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists2.Id,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Name = exists1.{entityName}Name,");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.AlreadyExists, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal($""'{{exists1.{entityName}Name.ToUpper()}}' already exists"", exception.Status.Detail);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.NotNull(exception.Trailers.Get(nameof({requestType.ConvertToCamelCase()}.{entityName}Name)));");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(exists1.{entityName}Name.ToUpper(),exception.Trailers.GetValue(nameof({options.ApiGrpcNamespace}.{requestType}.{entityName}Name)));");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithInvalidArgumentRequest_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Name = ""  "",");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""The provided request is invalid"", exception.Status.Detail);");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
            }
            else if (method.Name.Contains("Rollback") || method.Name.Contains("Restore"))
            {
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithValidRequest_ShouldSucceed()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine($@"{"\t\t"}await client.SoftDeleteAsync({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{responseType} {responseType.ConvertToCamelCase()} = await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.NotNull({responseType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal({options.ApiGrpcNamespace}.{entityName}StatusType.Active, {responseType.ConvertToCamelCase()}.State);");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo add response asserts");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(@$"{"\t"}public async Task Test{method.Name}_WithNonExistingId_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = 9999,");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(""'9999' not found"", exception.Status.Detail);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""9999"", exception.Trailers.GetValue(""criteria""));");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithInvalidArgumentRequest_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"}_testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""The provided request is invalid"", exception.Status.Detail);");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
            }
            else if (method.Name.Contains("Delete") || method.Name.Contains("Remove"))
            {
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithValidRequest_ShouldSucceed()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id,");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{responseType} {responseType.ConvertToCamelCase()} = await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.NotNull({responseType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo add response asserts");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(@$"{"\t"}public async Task Test{method.Name}_WithNonExistingId_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = 9999,");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal(""'9999' not found"", exception.Status.Detail);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""9999"", exception.Trailers.GetValue(""criteria""));");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();

                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WithInvalidArgumentRequest_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.CoreNamespace}.{entityName} exists = new(){{ ");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}// todo complete test case data");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}}};");
                outputServiceTest.AppendLine(@$"{"\t\t"} _testContext.DbContextAccessor.{entityName}s.Add(exists);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t\t"}{entityName}Id = exists.Id");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(@$"{"\t\t"}RpcException exception =");
                outputServiceTest.AppendLine(
                    @$"{"\t\t\t"}await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""The provided request is invalid"", exception.Status.Detail);");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
            }
            else if (method.Name.Contains("Get"))
            {
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_ReturnsListWithOneResult_ShouldSucceed()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}var list = _testContext.DbContextAccessor.{entityName}s.ToList();");
                outputServiceTest.AppendLine(@$"{"\t\t"}_testContext.DbContextAccessor.{entityName}s.RemoveRange(list);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");

                MethodDescriptorProto? createMethod =
                    serviceType.Method.OrderBy(i => i.Name.Length).FirstOrDefault(i => i.Name.Contains("Create") || i.Name.Contains("Add"));
                outputServiceTest.AppendLine(@$"{"\t\t"}// clear data and add one record");
                string createResponseType = "// create response type";
                if (createMethod != null)
                {
                    string createRequestType = createMethod.InputType.Split(".").LastOrDefault() ?? string.Empty;
                    createResponseType = createMethod.OutputType.Split(".").LastOrDefault() ?? string.Empty;
                
                    outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{createRequestType} {createRequestType.ConvertToCamelCase()} = new()");
                    outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                    outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                    outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                    outputServiceTest.AppendLine();
                    outputServiceTest.AppendLine(
                        @$"{"\t\t"}{options.ApiGrpcNamespace}.{createResponseType}? {createResponseType.ConvertToCamelCase()} = await client.{createMethod.Name}Async({createRequestType.ConvertToCamelCase()});");
                    outputServiceTest.AppendLine();

                }

                if (requestType.Contains("Empty"))
                {
                    outputServiceTest.AppendLine(@$"{"\t\t"}{requestType} {requestType.ConvertToCamelCase()} = new();");
                }
                else
                {
                    outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                    outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                    outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                    outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                }
                
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{responseType}? {responseType.ConvertToCamelCase()} = await client.{method.Name}Async({requestType.ConvertToCamelCase()});");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Single({responseType.ConvertToCamelCase()}.Items);");
                outputServiceTest.AppendLine(@$"{"\t\t"}Assert.Equal({createResponseType.ConvertToCamelCase()}.{entityName}Id, {responseType.ConvertToCamelCase()}.Items.First().{entityName}Id);");
                outputServiceTest.AppendLine(@$"{"\t\t"}// todo add list column asserts");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
                
                
                outputServiceTest.AppendLine(@$"{"\t"}[Fact]");
                outputServiceTest.AppendLine(
                    @$"{"\t"}public async Task Test{method.Name}_WhenNotFound_ShouldFail()");
                outputServiceTest.AppendLine(@$"{"\t"}{{");
                outputServiceTest.AppendLine(@$"{"\t\t"}// Arrange");
                outputServiceTest.AppendLine(@$"{"\t\t"}var list = _testContext.DbContextAccessor.{entityName}s.ToList();");
                outputServiceTest.AppendLine(@$"{"\t\t"}_testContext.DbContextAccessor.{entityName}s.RemoveRange(list);");
                outputServiceTest.AppendLine(@$"{"\t\t"}await _testContext.DbContextAccessor.SaveChangesAsync();");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}{options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Client client = new(_testContext.Channel);");
                if (requestType.Contains("Empty"))
                {
                    outputServiceTest.AppendLine(@$"{"\t\t"}{requestType} {requestType.ConvertToCamelCase()} = new();");
                }
                else
                {
                    outputServiceTest.AppendLine(@$"{"\t\t"}{options.ApiGrpcNamespace}.{requestType} {requestType.ConvertToCamelCase()} = new()");
                    outputServiceTest.AppendLine(@$"{"\t\t"}{{");
                    outputServiceTest.AppendLine(@$"{"\t\t"}// todo complete request data");
                    outputServiceTest.AppendLine(@$"{"\t\t"}}};");
                }
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Act");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}RpcException exception = await Assert.ThrowsAsync<RpcException>(async () => await client.{method.Name}Async({requestType.ConvertToCamelCase()}));");
                outputServiceTest.AppendLine();
                outputServiceTest.AppendLine(@$"{"\t\t"}// Assert");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);");
                outputServiceTest.AppendLine(
                    @$"{"\t\t"}Assert.Equal(""'{entityName}' not found"", exception.Status.Detail);");
                outputServiceTest.AppendLine(@$"{"\t"}}}");
                outputServiceTest.AppendLine();
            }
        }

        outputServiceTest.AppendLine("}");

        return new CodeGeneratorResponse.Types.File()
        {
            Name = "Test-" + entityName + "-ServiceTests.cs",
            Content = outputServiceTest.ToString()
        };
    }
    
}