using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class TxtCodeGenerator : ICodeGenerationStrategy
{
    public string Name => "txt";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        StringBuilder output = new();

        WriteServices(protoFiles, output);
 
        WriteMessages(protoFiles, output);
 
        WriteEnums(protoFiles, output);
        
        return new[]
        {
            new CodeGeneratorResponse.Types.File()
            {
                Name = fileName + ".txt",
                Content = output.ToString()
            }
        };
    }
    private static void WriteServices(IEnumerable<FileDescriptorProto> protoFiles, StringBuilder output)
    {
        foreach (ServiceDescriptorProto? serviceType in protoFiles.SelectMany((x) => x.Service))
        {
            output.AppendLine($"service {serviceType.Name.SimplifyTypeName()}");

            foreach (MethodDescriptorProto? method in serviceType.Method)
            {
                output.AppendLine(@$"{"\t"}{method.OutputType.SimplifyTypeName()} {method.Name}({method.InputType.SimplifyTypeName()})");
            }
        }
    } 

    private void WriteMessages(IEnumerable<FileDescriptorProto> protoFiles, StringBuilder output)
    {
        WriteTypes(protoFiles
            .Where(i => i.Service.Any())
            .SelectMany((x) => x.MessageType), 0, output);
    }

    private void WriteTypes(IEnumerable<DescriptorProto> messageTypes, int indent, StringBuilder output)
    {
        foreach (DescriptorProto messageType in messageTypes)
        {
            output.AppendLine(new string(' ', indent * 4) + $"message {messageType.Name.SimplifyTypeName()}");

            foreach (FieldDescriptorProto? field in messageType.Field)
            {
                output.AppendLine(new string(' ', indent * 4) +
                                  @$"{"\t"}{(!string.IsNullOrWhiteSpace(field.TypeName) ? field.TypeName.SimplifyTypeName() : field.Type.ToString().SimplifyTypeName())} {field.Name}");
            }

            if (messageType.NestedType is not { Count: > 0 }) continue;
            WriteTypes(messageType.NestedType, indent + 1, output);
        }
    }
    private static void WriteEnums(IEnumerable<FileDescriptorProto> protoFiles, StringBuilder output)
    {
        foreach (EnumDescriptorProto? enumDescriptorProto in protoFiles
                     .Where(i => i.Service.Any())
                     .SelectMany(i => i.EnumType))
        {
            output.AppendLine($"enum {enumDescriptorProto.Name.SimplifyTypeName()}");

            foreach (EnumValueDescriptorProto? valueDescriptor in enumDescriptorProto.Value)
            {
                output.AppendLine(@$"{"\t"}{valueDescriptor.Name}");
            }
        }
    }

}