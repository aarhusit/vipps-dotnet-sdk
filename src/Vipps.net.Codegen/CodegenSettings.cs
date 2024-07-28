using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace Vipps.net.Codegen
{
    internal sealed class CodegenSettings
    {
        internal string OpenApiUrl => $"https://developer.vippsmobilepay.com/redocusaurus/{UrlName}-swagger-id.yaml";
        internal string? Name { get; init; }
        internal string UrlName { get; init; }
        internal string RelativeFilePathClient { get; init; }
        internal string RelativeFilePathService { get; init; }
        internal CSharpClientGeneratorSettings ClientGeneratorSettings { get; init; }
        internal CSharpControllerGeneratorSettings ServiceGeneratorSettings { get; init; }
        internal CodegenSettings(
            string name,
            string urlName,
            string relativeFilePath
        )
        {
            Name = name;
            UrlName = urlName;
            RelativeFilePathClient = Path.Combine(relativeFilePath, $"{Name}Models.cs");
            RelativeFilePathService = Path.Combine(relativeFilePath, "Services", $"{Name}Service.cs");
            //ClientGeneratorSettings = new CSharpClientGeneratorSettings
            //{
            //    ClassName = $"Vipps{name}",
            //    CSharpGeneratorSettings =
            //    {
            //        Namespace = $"Vipps.net.Models.{name}",
            //        JsonLibrary = CSharpJsonLibrary.SystemTextJson
            //    },
            //};
            ClientGeneratorSettings = new CSharpClientGeneratorSettings
            {
                ClassName = $"Vipps{name}",
                GenerateClientClasses = true, //Generate ServiceMethods!
                //SuppressClientClassesOutput = false,
                OperationNameGenerator = new SingleClientFromOperationIdOperationNameGenerator(),
                GeneratePrepareRequestAndProcessResponseAsAsyncMethods = false,
                UseRequestAndResponseSerializationSettings = false,
                //SerializeTypeInformation = false,
                QueryNullValue = null,
                ExposeJsonSerializerSettings = false,
                CSharpGeneratorSettings =
                {
                    Namespace = $"Vipps.net.Models.{name}",
                    TypeAccessModifier = "public",
                    GenerateDataAnnotations = true,
                    GenerateDefaultValues = true,
                    JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                    TypeNameGenerator = new DefaultTypeNameGenerator(),
                    GenerateJsonMethods = false
                },
                //GenerateExceptionClasses = false,
                ////ExceptionClass = null,
                //InjectHttpClient = false,
                //DisposeHttpClient = false,
                ////ProtectedMethods = new string[] { },
                UseHttpClientCreationMethod = false,
                UseHttpRequestMessageCreationMethod = false,
                WrapDtoExceptions = false,
                ////ClientClassAccessModifier = null,
                //UseBaseUrl = false,
                //GenerateBaseUrlProperty = false,
                GenerateSyncMethods = false,
                //HttpClientType = null,
                //ParameterDateTimeFormat = null,
                //ParameterDateFormat = null,
                GenerateUpdateJsonSerializerSettingsMethod = false,
                GenerateDtoTypes = true,
                GenerateClientInterfaces = true,
                SuppressClientInterfacesOutput = false,
                GenerateOptionalParameters = true,
                ParameterNameGenerator = new DefaultParameterNameGenerator(),
                ////ExcludedParameterNames = new string[] { },
                WrapResponses = false,
                ////WrapResponseMethods = new string[] { },
                GenerateResponseClasses = false,
                ////ResponseClass = null,
                //AdditionalNamespaceUsages = new[] { "System.Jens" },
                //AdditionalContractNamespaceUsages = new  []{},
                ////ResponseArrayType = null,
                ////ResponseDictionaryType = null,
                ////ParameterArrayType = null,
                ////ParameterDictionaryType = null,
                //ClientBaseClass = null,
                ////ClientBaseInterface = null,
                ////ConfigurationClass = null,
            };
        }
    }
}
