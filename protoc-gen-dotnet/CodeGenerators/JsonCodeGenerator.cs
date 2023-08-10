using System.Text;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;

namespace protoc_gen_dotnet.CodeGenerators;

public class JsonCodeGenerator : ICodeGenerationStrategy
{
    public string Name => "json";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        StringBuilder outputJson = new();
        foreach (FileDescriptorProto fileDescriptorProto in protoFiles.Where(i => i.Service.Any()))
        {
            string? jsonString = JsonFormatter.Default.Format(fileDescriptorProto);
            outputJson.AppendLine(jsonString);
        }

        return new[]
        {
            new CodeGeneratorResponse.Types.File()
            {
                Name = fileName + ".json",
                Content = outputJson.ToString()
            }
        };
    }

    public string FileExtension => "json";
}