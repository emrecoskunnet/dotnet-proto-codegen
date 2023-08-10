using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;

namespace protoc_gen_dotnet;

public interface  ICodeGenerationStrategy
{
    string Name { get; }
    IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName, CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles);
}


public class CodeGenerationStrategyOptions
{
    public CodeGenerationStrategyOptions( string? coreNamespace)
    {
        CoreNamespace = coreNamespace;
    }
    public string? CoreNamespace { get; private set; }
    public string ApiNamespace => CoreNamespace + ".Api";
    public string ApiGrpcNamespace => CoreNamespace + ".Api.Grpc";
    public string? BaseName
    {
        get
        {
            string[]? l = CoreNamespace?.Replace(".Core", "").Split(".");
            return l?[^1];
        }
    }
}