using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;

namespace Vipps.net.Codegen
{
    internal sealed class CodegenSettings
    {
        internal string OpenApiUrl => $"https://developer.vippsmobilepay.com/redocusaurus/{Name?.ToLowerInvariant()}-swagger-id.yaml";
        internal string? Name { get; init; }
        internal string RelativeFilePath { get; init; }
        internal CSharpClientGeneratorSettings ClientGeneratorSettings { get; init; }
        internal CodegenSettings(
            string name,
            string relativeFilePath
        )
        {
            Name = name;
            RelativeFilePath = Path.Combine(relativeFilePath);
            ClientGeneratorSettings = new CSharpClientGeneratorSettings
            {
                ClassName = $"Vipps{name}",
                GenerateClientClasses = false,
                GeneratePrepareRequestAndProcessResponseAsAsyncMethods = false,
                CSharpGeneratorSettings =
                {
                    Namespace = $"Vipps.net.Models.{name}",
                    TypeAccessModifier = "public",
                    GenerateDataAnnotations = false,
                    GenerateDefaultValues = true,
                    JsonLibrary = CSharpJsonLibrary.SystemTextJson

                },
                GenerateExceptionClasses = false,
                GenerateBaseUrlProperty = false,
                GenerateUpdateJsonSerializerSettingsMethod = false,
                GenerateDtoTypes = true,
                GenerateOptionalParameters = true,
            };
        }
    }
}
