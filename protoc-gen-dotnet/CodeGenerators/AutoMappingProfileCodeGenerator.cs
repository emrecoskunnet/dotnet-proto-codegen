using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class AutoMappingProfileCodeGenerator : ICodeGenerationStrategy
{
    public string Name => "mapper";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName, CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        string protoName = fileName.Replace(".proto", "");
        StringBuilder output = new();
        output.AppendLine($"namespace {options.ApiNamespace}.Services;");
        output.AppendLine();
        output.AppendLine($"public class AutoMappingProfile{protoName} : AutoMapper.Profile");
        output.AppendLine("{");
        output.AppendLine($@"{"\t"}public AutoMappingProfile{protoName}()");
        output.AppendLine($@"{"\t"}{{");
        
        var createRequestTypeNames = protoFiles
            .SelectMany((x) => x.Service.SelectMany(l => l.Method))
            .Where(i => i.Name.Contains("Create"))
            .Select(methodDescriptor => methodDescriptor.InputType.Split(".").LastOrDefault())
            .ToList();

        var requestTypes = protoFiles.SelectMany(i => i.MessageType)
            .Where(i => createRequestTypeNames.Exists(l => l ==i.Name))
            .ToList();

        foreach (string requestTypeName in requestTypes.Select(i => i.Name))
        {
            string entityTypeName = requestTypeName.Replace("Request", "");
            output.AppendLine($@"{"\t\t"}CreateMap<{options.ApiGrpcNamespace}.{requestTypeName}, {options.CoreNamespace}.{entityTypeName}>()");
            output.AppendLine($@"{"\t\t\t"}.ConvertUsing( ({requestTypeName.ConvertToCamelCase()}, _, _) => new {options.CoreNamespace}.{entityTypeName}(");
            output.AppendLine($@"{"\t\t\t\t"} // todo Complete ctor call");
            output.AppendLine($@"{"\t\t\t"}){{ Id = {requestTypeName.ConvertToCamelCase()}.{entityTypeName}Id ?? default }});");
        }
        output.AppendLine();
        
        var createResultTypeNames = protoFiles
            .SelectMany((x) => x.Service.SelectMany(l => l.Method))
            .Where(i => i.Name.Contains("Create"))
            .Select(methodDescriptor => methodDescriptor.OutputType.Split(".").LastOrDefault())
            .ToList();
        
        var resultTypes = protoFiles.SelectMany(i => i.MessageType)
            .Where(i => createResultTypeNames.Exists(l => l ==i.Name))
            .ToList();

        foreach (string resultTypeName in resultTypes.Select(i => i.Name))
        {
            string entityTypeName = resultTypeName;
            output.AppendLine($@"{"\t\t"}CreateMap<{options.CoreNamespace}.{entityTypeName}, {options.ApiGrpcNamespace}.{resultTypeName}>()");
            output.AppendLine($@"{"\t\t\t"}.ForMember(i => i.{entityTypeName}Id, o => o.MapFrom(l => l.Id))");
            output.AppendLine($@"{"\t\t\t\t"} // todo Complete member mappings");
        }
        output.AppendLine();
        
        var listQueryTypeNames = protoFiles
            .SelectMany((x) => x.Service.SelectMany(l => l.Method))
            .Where(i => i.Name.Contains("Get") && i.Name.Contains("List"))
            .Select(methodDescriptor => methodDescriptor.InputType.Split(".").LastOrDefault())
            .ToList();
        
        var listQueryTypes = protoFiles.SelectMany(i => i.MessageType)
            .Where(i => listQueryTypeNames.Exists(l => l ==i.Name))
            .ToList();
          
        foreach (string listQueryName in listQueryTypes.Select(i => i.Name))
        {
            output.AppendLine($@"{"\t\t"}CreateMap<{options.ApiGrpcNamespace}.{listQueryName}, SharedKernel.Querying.ListInputModel>();");
        }
        
        output.AppendLine();
        
        var enums = protoFiles
            .SelectMany(i => i.EnumType)
            .ToList();

        foreach (string enumName in enums.Select(i => i.Name))
        {
            output.AppendLine($@"{"\t\t"}CreateMap<{options.CoreNamespace}.Enums.{enumName}, {options.ApiGrpcNamespace}.{enumName}>()");
            output.AppendLine($@"{"\t\t\t"}.ConstructUsing(i => ({options.ApiGrpcNamespace}.{enumName})(i.Value));");
            output.AppendLine();
            output.AppendLine($@"{"\t\t"}CreateMap<{options.ApiGrpcNamespace}.{enumName}, {options.CoreNamespace}.Enums.{enumName}>()");
            output.AppendLine(@$"{"\t\t\t"}.ConstructUsing(i => {options.CoreNamespace}.Enums.{enumName}.FromValue((int)i));");
            output.AppendLine();
        }
       
        
        var dtoServices = protoFiles
            .SelectMany((x) => x.Service.SelectMany(l => l.Method))
            .Where(i => i.Name.Contains("Get"))
            .Select(methodDescriptor => methodDescriptor.OutputType)
            .ToList();
        
        var dtoItemsTypes = protoFiles.SelectMany(i => i.MessageType)
            .Where(i => dtoServices.Exists(l => l.Contains(i.Name)))
            .SelectMany(i => i.NestedType);

        foreach (string dtoProtoName in dtoItemsTypes.Select(i => i.Name))
        {
            string dtoName = dtoProtoName + "Dto";
            string apiDtoName = $"{dtoProtoName.Replace("Item", "")}.Types.{dtoProtoName}";

            string entityTypeName = protoName[..^2];
            
            if(dtoName.Contains("Lookup"))
                output.AppendLine($@"{"\t\t"}CreateMap<{options.CoreNamespace}.Projections.{dtoName}, {options.ApiGrpcNamespace}.{apiDtoName}>();");
            else if (dtoName.Contains("List"))
            {
                output.AppendLine($@"{"\t\t"}CreateMap<{options.CoreNamespace}.Projections.{dtoName}, {options.ApiGrpcNamespace}.{apiDtoName}>();");
            }
            else
            {
                output.AppendLine($@"{"\t\t"}CreateMap<{options.CoreNamespace}.Projections.{dtoName}, {options.ApiGrpcNamespace}.{apiDtoName}>();");
                output.AppendLine($@"{"\t\t"}// todo add custom mappings");
            }
            
            output.AppendLine();
        }
        output.AppendLine($@"{"\t\t"}// todo add DateTime mapping if not exists");
        output.AppendLine($@"{"\t\t"}// CreateMap<DateTime, Google.Protobuf.WellKnownTypes.Timestamp>()");
        output.AppendLine($@"{"\t\t\t"}// .ConvertUsing(l => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(l));");
        output.AppendLine($@"{"\t\t"}// CreateMap<Google.Protobuf.WellKnownTypes.Timestamp, DateTime>()");
        output.AppendLine($@"{"\t\t\t"}// .ConvertUsing(l => l.ToDateTime());");

        output.AppendLine($@"{"\t"}}}");
        output.AppendLine("}");
        
        yield return new CodeGeneratorResponse.Types.File()
        {
            Name = "Api-Services-AutoMappingProfile" + protoName + ".cs",
            Content = output.ToString()
        };
    }
}