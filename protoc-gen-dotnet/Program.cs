using Google.Protobuf;
using Google.Protobuf.Compiler;
using protoc_gen_dotnet;
using protoc_gen_dotnet.CodeGenerators;

internal static class Program
{
    private static readonly List<ICodeGenerationStrategy> GenerationStrategies = new()
    {
        new TxtCodeGenerator(), new JsonCodeGenerator(), new DtoCodeGenerator(),
        new ServiceCodeGenerator(), new AutoMappingProfileCodeGenerator(), 
        new ServiceTestCodeGenerator(), new ClientServiceProxyCodeGenerator()
    };

    // ReSharper disable once UnusedParameter.Local
    static void Main(string[] args)
    {
        // get request from standard input
        CodeGeneratorRequest request;
        using (Stream stdin = Console.OpenStandardInput())
        {
            request = Deserialize<CodeGeneratorRequest>(stdin);
        }

        CodeGeneratorResponse response = new();
        var generatorNames = request.Parameter.Split(",").Where(i => i.StartsWith("generator="))
            .Select(i => i.Replace("generator=", "")).ToList();

        if (!generatorNames.Any()) generatorNames.AddRange(GenerationStrategies.Select(i => i.Name));


        string? coreNamespace = request.Parameter
            .Split(",").ToList().Find(i => i.StartsWith("core-namespace="))
            ?.Replace("core-namespace=", "");

        CodeGenerationStrategyOptions options = new(coreNamespace);
        foreach (string? file in request.FileToGenerate)
        {
            foreach (var generatorResult in
                     from generatorName in generatorNames
                     select GenerationStrategies.ToList().Find(i => i.Name == generatorName)
                     into generator
                     where generator != null
                     select generator.WriteOutput(file, options, request.ProtoFile.ToList()))
            {
                response.File.AddRange(generatorResult);
            }
        }


        // set result to standard output
        using (Stream stdout = Console.OpenStandardOutput())
        {
            response.WriteTo(stdout);
        }
    }

    static T Deserialize<T>(Stream stream) where T : IMessage<T>, new()
        => new MessageParser<T>(() => new T()).ParseFrom(stream);
}