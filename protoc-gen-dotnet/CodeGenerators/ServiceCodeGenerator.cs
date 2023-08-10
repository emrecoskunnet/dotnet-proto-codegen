using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class ServiceCodeGenerator : ICodeGenerationStrategy
{
    public string Name => "srv";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        return from serviceType in protoFiles.SelectMany((x) => x.Service)
            let entityName = serviceType.Name.Replace("Service", "")
            select ServiceCodeGeneration(options, entityName, serviceType);
    }

    private static CodeGeneratorResponse.Types.File ServiceCodeGeneration(CodeGenerationStrategyOptions options,
        string entityName,
        ServiceDescriptorProto serviceType)
    {
        StringBuilder outputService = new();
        outputService.AppendLine("using AutoMapper;");
        outputService.AppendLine("using Google.Protobuf.WellKnownTypes;");
        outputService.AppendLine("using Grpc.Core;");
        outputService.AppendLine("using Microsoft.AspNetCore.Authorization;");
        outputService.AppendLine();
        outputService.AppendLine(@$"namespace {options.ApiNamespace}.Services;");
        outputService.AppendLine();
        outputService.AppendLine("[Authorize]");
        outputService.AppendLine("// ReSharper disable once ClassNeverInstantiated.Global");
        outputService.AppendLine(
            $"public class {entityName}V1Service : {options.ApiGrpcNamespace}.{serviceType.Name}.{serviceType.Name}Base");
        outputService.AppendLine("{");
        outputService.AppendLine(@$"{"\t"}private readonly IAppLocalizer _localizer;");
        outputService.AppendLine(@$"{"\t"}private readonly IMapper _mapper;");
        outputService.AppendLine(
            @$"{"\t"}private readonly IRepository<{options.CoreNamespace}.{entityName}> _repository;");
        outputService.AppendLine();
        outputService.AppendLine(
            @$"{"\t"}public {entityName}V1Service(IAppLocalizer localizer, IMapper mapper, IRepository<{options.CoreNamespace}.{entityName}> repository)");
        outputService.AppendLine(@$"{"\t"}{{");
        outputService.AppendLine($@"{"\t\t"}_localizer = localizer;");
        outputService.AppendLine($@"{"\t\t"}_mapper = mapper;");
        outputService.AppendLine($@"{"\t\t"}_repository = repository;");
        outputService.AppendLine(@$"{"\t"}}}");
        outputService.AppendLine();
        foreach (MethodDescriptorProto? method in serviceType.Method)
        {
            string requestType = method.InputType.Split(".").LastOrDefault() ?? string.Empty;
            string responseType = method.OutputType.Split(".").LastOrDefault() ?? string.Empty;
            if (method.Name.Contains("Get"))
            {
                outputService.AppendLine();
                outputService.AppendLine(
                    @$"{"\t"}public override async Task<{options.ApiGrpcNamespace}.{responseType}> {method.Name}({options.ApiGrpcNamespace}.{requestType} request, ServerCallContext context)");
                outputService.AppendLine(@$"{"\t"}{{");
                outputService.AppendLine(@$"{"\t\t"}try");
                outputService.AppendLine(@$"{"\t\t"}{{");

                switch (method.Name)
                {
                    case "GetLookup":
                        outputService.AppendLine(@$"{"\t\t\t"}var list = await _repository.ListAsync(");
                        outputService.AppendLine(
                            @$"{"\t\t\t\t"}request,");
                        outputService.AppendLine(@$"{"\t\t\t\t"}context.CancellationToken);");
                        outputService.AppendLine();
                        outputService.AppendLine(@$"{"\t\t\t"}{responseType} result = new()");
                        outputService.AppendLine(@$"{"\t\t\t"}{{");
                        outputService.AppendLine(@$"{"\t\t\t\t"}Total = list.Count");
                        outputService.AppendLine(@$"{"\t\t\t"}}};");
                        outputService.AppendLine(
                            @$"{"\t\t\t"}result.Items.AddRange(list.Select(i => _mapper.Map<{options.ApiGrpcNamespace}.{responseType}.Types.{responseType}Item>(i)));");
                        outputService.AppendLine(@$"{"\t\t\t"}return result;");
                        break;
                    case "GetList":
                        outputService.AppendLine(
                            @$"{"\t\t\t"}ListInputModel? inputModel = _mapper.Map<{options.ApiGrpcNamespace}.{requestType}, ListInputModel>(request);");
                        outputService.AppendLine(@$"{"\t\t\t"}var pagedList = await _repository.PagedListAsync(");
                        outputService.AppendLine(
                            @$"{"\t\t\t\t"}request,");
                        outputService.AppendLine(@$"{"\t\t\t\t"}inputModel,");
                        outputService.AppendLine(@$"{"\t\t\t\t"}context.CancellationToken);");
                        outputService.AppendLine();
                        outputService.AppendLine(
                            @$"{"\t\t\t"}{options.ApiGrpcNamespace}.{responseType} result = new()");
                        outputService.AppendLine(@$"{"\t\t\t"}{{");
                        outputService.AppendLine(@$"{"\t\t\t\t"}Total = pagedList.Total");
                        outputService.AppendLine(@$"{"\t\t\t"}}};");
                        outputService.AppendLine(
                            @$"{"\t\t\t"}result.Items.AddRange(pagedList.Select(i => _mapper.Map<{options.ApiGrpcNamespace}.{responseType}.Types.{responseType}Item>(i)));");
                        outputService.AppendLine(@$"{"\t\t\t"}return result;");
                        break;
                    case "GetById":
                        outputService.AppendLine(
                            @$"{"\t\t\t"}{options.CoreNamespace}.{responseType} exists = await _repository.SingleAsync(");
                        outputService.AppendLine(
                            @$"{"\t\t\t\t"}request,");
                        outputService.AppendLine(@$"{"\t\t\t\t"}context.CancellationToken);");
                        outputService.AppendLine(
                            @$"{"\t\t\t"}return _mapper.Map<{options.ApiGrpcNamespace}.{responseType}>(exists);");
                        break;
                }

                outputService.AppendLine(@$"{"\t\t"}}}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (DomainObjectNotFoundException e) {{ throw new RpcException(new Status(StatusCode.NotFound, _localizer[""'{0}' not found"", request.GetType().Name])); }}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (Exception e){{ throw new RpcException(new Status(StatusCode.Unknown, localizer[""An error occurred while processing the request""])); }}");
                outputService.AppendLine(@$"{"\t"}}}");
            }
            else if (method.Name.Contains("Create") || method.Name.Contains("Add"))
            {
                outputService.AppendLine();
                outputService.AppendLine(
                    $@"{"\t"}public override async Task<{options.ApiGrpcNamespace}.{responseType}> {method.Name}({options.ApiGrpcNamespace}.{requestType} request,");
                outputService.AppendLine($@"{"\t\t"}ServerCallContext context)");
                outputService.AppendLine($@"{"\t"}{{");
                outputService.AppendLine($@"{"\t\t"}try");
                outputService.AppendLine($@"{"\t\t"}{{");
                outputService.AppendLine(
                    $@"{"\t\t\t"}{options.CoreNamespace}.{responseType} {responseType?.ConvertToCamelCase()} =");
                outputService.AppendLine(
                    $@"{"\t\t\t\t"}await _repository.AddAsync(_mapper.Map<{options.CoreNamespace}.{responseType}>(request),");
                outputService.AppendLine($@"{"\t\t\t\t"}context.CancellationToken);");
                outputService.AppendLine();
                outputService.AppendLine(
                    $@"{"\t\t\t"}return _mapper.Map<{options.ApiGrpcNamespace}.{responseType}>({responseType?.ConvertToCamelCase()});");
                outputService.AppendLine($@"{"\t\t"}}}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (DomainObjectNotFoundException e) {{ throw new RpcException(new Status(StatusCode.NotFound, _localizer[""'{0}' not found"", request.GetType().Name])); }}");
                outputService.AppendLine(
                    $@"{"\t\t"}catch (ArgumentException e) {{ throw new RpcException(new Status(StatusCode.InvalidArgument, localizer[""The provided request is invalid""])); }}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (Exception e){{ throw new RpcException(new Status(StatusCode.Unknown, localizer[""An error occurred while processing the request""])); }}");
                outputService.AppendLine(@$"{"\t"}}}");
            }
            else if (method.Name.Contains("Update") || method.Name.Contains("Edit"))
            {
                outputService.AppendLine();
                outputService.AppendLine(
                    $@"{"\t"}public override async Task<{options.ApiGrpcNamespace}.{responseType}> {method.Name}({options.ApiGrpcNamespace}.{requestType} request,");
                outputService.AppendLine($@"{"\t\t"}ServerCallContext context)");
                outputService.AppendLine($@"{"\t"}{{");
                outputService.AppendLine($@"{"\t\t"}try");
                outputService.AppendLine($@"{"\t\t"}{{");
                outputService.AppendLine(
                    $@"{"\t\t\t"}{options.CoreNamespace}.{responseType} exists = await _repository.SingleAsync(new {responseType}Spec(request.{responseType}Id ?? 0, {responseType}Filter.Default));");
                outputService.AppendLine($@"{"\t\t\t"}exists.Update(); // todo complete update code");
                outputService.AppendLine(
                    $@"{"\t\t\t"}await _repository.SaveChangesAsync(context.CancellationToken);");
                outputService.AppendLine();
                outputService.AppendLine(
                    $@"{"\t\t\t"}return _mapper.Map<{options.ApiGrpcNamespace}.{responseType}>(exists);");
                outputService.AppendLine($@"{"\t\t"}}}");
                outputService.AppendLine(
                    $@"{"\t\t"}catch (DomainObjectAlreadyExistsException e) {{ throw new RpcException(new Status(StatusCode.AlreadyExists, localizer[""'{0}' already exists"", request.GetType().Name])); }}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (DomainObjectNotFoundException e) {{ throw new RpcException(new Status(StatusCode.NotFound, _localizer[""'{0}' not found"", request.GetType().Name])); }}");
                outputService.AppendLine(
                    $@"{"\t\t"}catch (ArgumentException e) {{ throw new RpcException(new Status(StatusCode.InvalidArgument, localizer[""The provided request is invalid""])); }}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (Exception e){{ throw new RpcException(new Status(StatusCode.Unknown, localizer[""An error occurred while processing the request""])); }}");
                outputService.AppendLine(@$"{"\t"}}}");
            }
            else if (method.Name.Contains("Delete") || method.Name.Contains("Remove"))
            {
                outputService.AppendLine();
                outputService.AppendLine(
                    $@"{"\t"}public override async Task<{options.ApiGrpcNamespace}.{responseType}> {method.Name}({options.ApiGrpcNamespace}.{requestType} request,");
                outputService.AppendLine($@"{"\t\t"}ServerCallContext context)");
                outputService.AppendLine($@"{"\t"}{{");
                outputService.AppendLine($@"{"\t\t"}try");
                outputService.AppendLine($@"{"\t\t"}{{");
                outputService.AppendLine(
                    $@"{"\t\t\t"}{options.CoreNamespace}.{entityName} exists = await _repository.SingleAsync(new {entityName}Spec(request.{entityName}Id, {entityName}Filter.All));");
                outputService.AppendLine($@"{"\t\t\t"}exists.SoftDelete(); // todo complete delete code");
                outputService.AppendLine($@"{"\t\t\t"}await _repository.SaveChangesAsync(context.CancellationToken);");
                outputService.AppendLine();
                outputService.AppendLine($@"{"\t\t\t"}return new {responseType}();");
                outputService.AppendLine($@"{"\t\t"}}}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (DomainObjectNotFoundException e) {{ throw new RpcException(new Status(StatusCode.NotFound, _localizer[""'{0}' not found"", request.GetType().Name])); }}");
                outputService.AppendLine(
                    $@"{"\t\t"}catch (ArgumentException e) {{ throw new RpcException(new Status(StatusCode.InvalidArgument, localizer[""The provided request is invalid""])); }}");
                outputService.AppendLine(
                    @$"{"\t\t"}catch (Exception e){{ throw new RpcException(new Status(StatusCode.Unknown, localizer[""An error occurred while processing the request""])); }}");
                outputService.AppendLine(@$"{"\t"}}}");
            }
        }

        outputService.AppendLine("}");
        return new CodeGeneratorResponse.Types.File()
        {
            Name = "Api-Services-" + entityName + "-Service.cs",
            Content = outputService.ToString()
        };
    }
    
}