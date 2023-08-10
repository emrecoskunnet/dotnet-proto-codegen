using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class DtoCodeGenerator : ICodeGenerationStrategy
{
    public string Name => "dto";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        var getTypes = protoFiles
            .SelectMany((x) => x.Service.SelectMany(l => l.Method))
            .Where(i => i.Name.Contains("Get"))
            .Select(methodDescriptor => methodDescriptor.OutputType)
            .ToList();


        var itemsTypes = protoFiles.SelectMany(i => i.MessageType)
            .Where(i => getTypes.Exists(l => l.Contains(i.Name)))
            .SelectMany(i => i.NestedType);
        foreach (DescriptorProto itemsType in itemsTypes)
        {
            string dtoName = itemsType.Name + "Dto";
            StringBuilder output = new();

            output.AppendLine($"namespace {options.CoreNamespace}.Projections;");
            output.AppendLine();
            output.AppendLine($"public class {dtoName}");
            output.AppendLine("{");

            
            foreach (FieldDescriptorProto fieldDescriptorProto in itemsType.Field)
            {
                output.AppendLine(
                    $@"{"\t"}public {(fieldDescriptorProto.Label == FieldDescriptorProto.Types.Label.Repeated ? "IEnumerable<" : "")}{fieldDescriptorProto.Type.ConvertGrpcTypeToClrType(fieldDescriptorProto.TypeName)}{(fieldDescriptorProto.Label == FieldDescriptorProto.Types.Label.Repeated ? ">?" : "")} {fieldDescriptorProto.Name.ConvertCamelCaseToPascalCase()}  {{ get; set; }} {(fieldDescriptorProto.Type == FieldDescriptorProto.Types.Type.String ? " = string.Empty;" : "")}");
            }

            output.AppendLine("}");

            yield return new CodeGeneratorResponse.Types.File()
            {
                Name = "Core-Projections-" +dtoName + ".cs",
                Content = output.ToString()
            };
        }
    }
}