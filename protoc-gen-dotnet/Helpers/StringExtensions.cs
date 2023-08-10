using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.Reflection;

namespace protoc_gen_dotnet.Helpers;

public static partial class StringExtensions
{
    public static string SimplifyTypeName(this string input)
    {
        int index = input.Contains('.') ? input.LastIndexOf('.') + 1 : 0;
        return input[index..];
    }

    public static string ConvertCamelCaseToPascalCase(this string input)
    {
        string output = PascalCaseConverterRegex().Replace(input, match => match.Value.ToUpper());
        return output;
    }

    [GeneratedRegex("\\b\\p{Ll}")]
    private static partial Regex PascalCaseConverterRegex();

    public static string ConvertKebabCaseToCamelCase(this string input)
    {
        StringBuilder sb = new();
        bool caseFlag = false;
        foreach (char c in input)
            if (c == '-')
            {
                caseFlag = true;
            }
            else if (caseFlag)
            {
                sb.Append(char.ToUpper(c));
                caseFlag = false;
            }
            else
            {
                sb.Append(char.ToLower(c));
            }

        return sb.ToString();
    }
    public static string ConvertPascalCaseToCamelCase(this string input)
    {
        StringBuilder sb = new();
        bool caseFlag = false;
        foreach (char c in input)
            if (caseFlag)
            {
                sb.Append(char.ToUpper(c));
                caseFlag = false;
            }
            else
            {
                sb.Append(char.ToLower(c));
                caseFlag = true;
            }

        return sb.ToString();
    }
    
    public static string ConvertToCamelCase(this string input)
    {
        if ( !string.IsNullOrEmpty(input) && char.IsUpper(input[0]))
            return input.Length == 1 ? char.ToLower(input[0]).ToString() : char.ToLower(input[0]) + input[1..];

        return input;
    }
    
    public static string ConvertGrpcTypeToClrType(this FieldDescriptorProto.Types.Type type, string typeName)
    {
        return type switch
        {
            FieldDescriptorProto.Types.Type.Int32 => "int",
            FieldDescriptorProto.Types.Type.String => "string",
            FieldDescriptorProto.Types.Type.Double => "double",
            FieldDescriptorProto.Types.Type.Float => "short",
            FieldDescriptorProto.Types.Type.Int64 => "long",
            FieldDescriptorProto.Types.Type.Bool => "boolean",
            FieldDescriptorProto.Types.Type.Enum => typeName.Split(".").LastOrDefault() ?? typeName ,
            FieldDescriptorProto.Types.Type.Message =>
                 typeName switch
                 {
                     ".google.protobuf.StringValue" => "string?",
                     ".google.protobuf.Timestamp" => "DateTime",
                     ".google.protobuf.DoubleValue" => "double?",
                     ".google.protobuf.Int32Value" => "int?",
                     _ => typeName
                 },
             _ => type.ToString()
        };
    }
}