_dotnet-proto-codegen_ is an open-source tool that automatically generates DTOs (Data Transfer Objects), service classes, and test code in dotnet (C#) projects using Protobuf (Protocol Buffers) files. With this tool, you can quickly and accurately generate code for data models and services in projects that utilize Protobuf files.

### Features:

- Full Compatibility: Supports all Protobuf files and is a specialized with protoc plugin for generating dotnet code.
- Flexible Templates: Users can use customizable templates for generating custom code tailored to their projects or modify existing templates.
- Fork and Customize: Users can fork this repository and add their own code generation routines to create code that meets their specific requirements.
- Rich Documentation: Users can quickly learn how to use the tool with explanatory documentation and examples.

### How to Get Started:

- Fork the repository and start your project in your account.
- Add your Protobuf files to the proto folder and customize templates if needed.
- Run protoc with the protoc-gen-dotnet plugin from the command line to generate DTOs, service classes, and test code.

This project aims to assist developers in working efficiently with Protobuf files in dotnet projects. Feel free to contribute or customize; together, we can grow the project through community collaboration and sharing.

### Usage:

_Build CodeGen_

 - Download code
```git clone https://github.com/emrecoskunnet/dotnet-proto-codegen.git ```
 - Dotnet restore & build 
```dotnet build ```

_Move Exec Path_
```cd protoc-gen-dotnet/bin/Debug/net7.0 ```

_Execute All Generators_

```
protoc --proto_path=/sample/  --plugin=proto-gen-dotnet --dotnet_out=/sample/ApiDocumentationTxt/  /sample/hello.proto
```


###  Parematers

- generator (--dotnet_opt=generator=) : Name of code generator. If not set or blank all generator will execute

  - txt : proto to text conversion for documentation purpose.
  - json: proto to json conversion. Direct call to protoc json serializer
  - dto: generates Dto for Get* services
  - srv: generates Service implementation
  - test: generates Test cases for Service implementation
  - proxy: generates Type Script proxy class for Services

- core-namespace: Namespace of generated files.(etc: Foo.Core)

### Important
 
- protoc works with full relative path. You must input full path like this:

```
protoc --proto_path=/Users/emrecoskun/Projects/dotnet-proto-codegen/samples/  --plugin=./protoc-gen-dotnet --dotnet_out=/Users/emrecoskun/Projects/dotnet-proto-codegen/samples/CodeGenerationOutput/ --dotnet_opt=core-namespace=Foo.Core  /Users/emrecoskun/Projects/dotnet-proto-codegen/samples/hello.proto
```

- These code generators are for example purposes only. Don't use it directly in your projects. They were written to give you an idea of how you can make a code generator suitable for your own project.


