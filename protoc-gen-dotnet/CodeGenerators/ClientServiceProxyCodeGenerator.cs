using System.Text;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using protoc_gen_dotnet.Helpers;

namespace protoc_gen_dotnet.CodeGenerators;

public class ClientServiceProxyCodeGenerator: ICodeGenerationStrategy
{
    public string Name => "proxy";

    public IEnumerable<CodeGeneratorResponse.Types.File> WriteOutput(string fileName,
        CodeGenerationStrategyOptions options, List<FileDescriptorProto> protoFiles)
    {
        List<CodeGeneratorResponse.Types.File> result = new();

        foreach (ServiceDescriptorProto? serviceType in protoFiles.SelectMany((x) => x.Service))
        {
            result.AddRange(ApiCodeGenerator(protoFiles, serviceType));
        }

        return result;
    }
 
    private static IEnumerable<CodeGeneratorResponse.Types.File> ApiCodeGenerator(IEnumerable<FileDescriptorProto> protoFiles,
        ServiceDescriptorProto serviceType)
    {
        string entityName = serviceType.Name.Replace("Service", "");
        StringBuilder outputApi = new();
        outputApi.AppendLine(@"import call from '@/config/grpcWeb'");
        outputApi.AppendLine("import {");
        outputApi.AppendLine(@$"{"\t"}{serviceType.Name}ClientImpl,");
        outputApi.AppendLine(@$"{"\t"}GrpcWebImpl,");
        outputApi.AppendLine(@$"{"\t"}{serviceType.Name},");
        foreach (DescriptorProto? message in protoFiles.Where(i => i.Service.Any()).SelectMany(i => i.MessageType))
        {
            outputApi.AppendLine(@$"{"\t"}{message.Name},");
        }

        outputApi.AppendLine(@$"}} from '@/api/{entityName.ConvertToCamelCase()}/{entityName}V1'");
        outputApi.AppendLine(@"import { TableResponse } from '@/hooks/web/useTable'");
        outputApi.AppendLine(@"import { ComponentOptions } from '@/types/components'");
        outputApi.AppendLine(
            @"const PATH_URL = import.meta.env.VITE_API_BASEPATH + 'XX'  // todo add api short link");
        outputApi.AppendLine();
        outputApi.AppendLine(@"const grpcWebImpl = new GrpcWebImpl(PATH_URL, {");
        outputApi.AppendLine(@$"{"\t"}debug: false,");
        outputApi.AppendLine(@$"{"\t"}metadata: call.authorizationMetadata()");
        outputApi.AppendLine(@"})");
        outputApi.AppendLine();
        outputApi.AppendLine(@$"export const {serviceType.Name}Client: {serviceType.Name} =");
        outputApi.AppendLine(@$"{"\t"}new {serviceType.Name}ClientImpl(grpcWebImpl)");


        foreach (MethodDescriptorProto? method in serviceType.Method)
        {
            string requestType = method.InputType.Split(".").LastOrDefault() ?? string.Empty;
            string responseType = method.OutputType.Split(".").LastOrDefault() ?? string.Empty;

            if (method.Name.Contains("GetLookup"))
            {
                outputApi.AppendLine(
                    $@"export const {entityName.ConvertToCamelCase()}{method.Name}Api = (): Promise<IResponse<{responseType}>> => {{");
                outputApi.AppendLine(
                    $@"{"\t"}return new Promise<IResponse<{responseType}>>((resolve, reject) => {{");
                outputApi.AppendLine(@$"{"\t\t"}{serviceType.Name}Client.GetLookup({{}})");
                outputApi.AppendLine(@$"{"\t\t\t"}.then((i) => {{");
                outputApi.AppendLine(@$"{"\t\t\t\t"}resolve({{");
                outputApi.AppendLine(@$"{"\t\t\t\t\t"}code: 'Ok',");
                outputApi.AppendLine(@$"{"\t\t\t\t\t"}data: i");
                outputApi.AppendLine(@$"{"\t\t\t\t"}}} as IResponse<{responseType}>)");
                outputApi.AppendLine(@$"{"\t\t\t"}}})");
                outputApi.AppendLine(@$"{"\t\t\t"}.catch(reject)");
                outputApi.AppendLine(@$"{"\t"}}})");
                outputApi.AppendLine(@"}");
                outputApi.AppendLine();
                outputApi.AppendLine(
                    $@"export const {entityName.ConvertCamelCaseToPascalCase()}{method.Name}ComponentOptionsApi = (): Promise<ComponentOptions[]> => {{");
                outputApi.AppendLine($@"{"\t"}return new Promise<ComponentOptions[]>((resolve, reject) => {{");
                outputApi.AppendLine($@"{"\t\t"}{entityName.ConvertToCamelCase()}{method.Name}Api()");
                outputApi.AppendLine($@"{"\t\t\t"}.then((i) => {{");
                outputApi.AppendLine($@"{"\t\t\t\t"}const options{entityName}: ComponentOptions[] = []");
                outputApi.AppendLine($@"{"\t\t\t\t"}options{entityName}.length = 0");
                outputApi.AppendLine($@"{"\t\t\t\t"}options{entityName}.push(");
                outputApi.AppendLine($@"{"\t\t\t\t\t"}...i.data.items.map((val) => ({{");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t"}value: val.{entityName.ConvertToCamelCase()}Id,");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t"}label: val.{entityName.ConvertToCamelCase()}Name");
                outputApi.AppendLine($@"{"\t\t\t\t\t"}}}))");
                outputApi.AppendLine($@"{"\t\t\t\t"})");
                outputApi.AppendLine($@"{"\t\t\t\t"}resolve(options{entityName})");
                outputApi.AppendLine($@"{"\t\t\t"}}})");
                outputApi.AppendLine($@"{"\t\t\t"}.catch(reject)");
                outputApi.AppendLine($@"{"\t"}}})");
                outputApi.AppendLine(@"}");
                outputApi.AppendLine();
            }
            else if (method.Name.Contains("GetList"))
            {
                outputApi.AppendLine($@"export const {entityName.ConvertToCamelCase()}{method.Name}Api = (");
                outputApi.AppendLine($@"{"\t"}params: {requestType}");
                outputApi.AppendLine(
                    $@"): Promise<IResponse<TableResponse<{responseType}_{responseType}Item>>> => {{");
                outputApi.AppendLine(
                    $@"{"\t"}return new Promise<IResponse<TableResponse<{responseType}_{responseType}Item>>>(");
                outputApi.AppendLine($@"{"\t\t"}(resolve, reject) => {{");
                outputApi.AppendLine($@"{"\t\t\t"}{serviceType.Name}Client.{method.Name}(params)");
                outputApi.AppendLine($@"{"\t\t\t\t"}.then((i) => {{");
                outputApi.AppendLine($@"{"\t\t\t\t\t"}resolve({{");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t"}code: 'Ok',");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t"}data: {{");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t\t"}total: i.total,");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t\t"}list: i.items,");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t\t"}pageNumber: params.pageNumber,");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t\t"}pageSize: params.pageSize");
                outputApi.AppendLine($@"{"\t\t\t\t\t\t"}}}");
                outputApi.AppendLine(
                    $@"{"\t\t\t\t\t"}}} as IResponse<TableResponse<{responseType}_{responseType}Item>>)");
                outputApi.AppendLine($@"{"\t\t\t\t"}}})");
                outputApi.AppendLine($@"{"\t\t\t\t"}.catch(reject)");
                outputApi.AppendLine($@"{"\t\t"}}}");
                outputApi.AppendLine($@"{"\t"})");
                outputApi.AppendLine(@"}");
                outputApi.AppendLine();
            }
            else if (method.Name.Contains("Rollback") || method.Name.Contains("Remove") ||
                     method.Name.Contains("Delete") || method.Name.Contains("Restore"))
            {
                outputApi.AppendLine(
                    @$"export const {entityName.ConvertToCamelCase()}{method.Name}Api = (ids: number[]): Promise<IResponse> => {{");
                outputApi.AppendLine(@$"{"\t"}return new Promise<IResponse>((resolve, reject) => {{");
                outputApi.AppendLine(@$"{"\t\t"}{serviceType.Name}Client.{method.Name}({{");
                outputApi.AppendLine(@$"{"\t\t\t"}{entityName.ConvertToCamelCase()}Id: ids[0]");
                outputApi.AppendLine(@$"{"\t\t"}}} as {requestType})");
                outputApi.AppendLine(@$"{"\t\t\t"}.then((i) => resolve({{ code: 'OK', data: i }}))");
                outputApi.AppendLine(@$"{"\t\t\t"}.catch(reject)");
                outputApi.AppendLine(@$"{"\t"}}})");
                outputApi.AppendLine(@"}");
            }
            else
            {
                outputApi.AppendLine(@$"export const {entityName.ConvertToCamelCase()}{method.Name}Api = (");
                outputApi.AppendLine(@$"{"\t"}params: {requestType}");
                outputApi.AppendLine(@$"): Promise<IResponse<{responseType}>> => {{");
                outputApi.AppendLine(
                    @$"{"\t"}return new Promise<IResponse<{responseType}>>((resolve, reject) => {{");
                outputApi.AppendLine(@$"{"\t\t"}{serviceType.Name}Client.{method.Name}(params)");
                outputApi.AppendLine(@$"{"\t\t\t"}.then((i) => resolve({{ code: 'OK', data: i }}))");
                outputApi.AppendLine(@$"{"\t\t\t"}.catch(reject)");
                outputApi.AppendLine(@$"{"\t"}}})");
                outputApi.AppendLine(@"}");
            }
        }

        yield return new CodeGeneratorResponse.Types.File()
        {
            Name = $@"Web-Api-{entityName}-index.ts",
            Content = outputApi.ToString()
        };
    }
}