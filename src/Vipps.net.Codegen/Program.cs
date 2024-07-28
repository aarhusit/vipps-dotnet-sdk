using System.Dynamic;
using System.Text.Json;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using Vipps.net.Codegen;

internal sealed class Program
{
    private static async Task Main()
    {
        var httpClient = new HttpClient();
        const string relativeFilePath = "../Vipps.net/Models/";

        //Epayment
        await GenerateCode(httpClient, new CodegenSettings("Epayment", "epayment", relativeFilePath));

        //Checkout
        await GenerateCode(httpClient, new CodegenSettings("Checkout", "checkout", relativeFilePath));

        //Recurring
        await GenerateCode(httpClient, new CodegenSettings("Recurring", "recurring", relativeFilePath));

        //Login
        //var loginOptions = new CodegenSettings("Login", Path.Combine(relativeFilePath, "LoginModels.cs"));
        //await GenerateCode(httpClient, loginOptions);

        //AccessToken
        await GenerateCode(httpClient, new CodegenSettings("AccessToken", "access-token", relativeFilePath));

        //CheckIn
        await GenerateCode(httpClient, new CodegenSettings("CheckIn", "check-in", relativeFilePath));

        //Management
        await GenerateCode(httpClient, new CodegenSettings("Management", "management", relativeFilePath));
    }

    private static async Task GenerateCode(HttpClient httpClient, CodegenSettings options)
    {
        Console.WriteLine($"Fetching from {options.OpenApiUrl}");
        var retrievedText = await httpClient.GetStringAsync(options.OpenApiUrl);
        //Console.WriteLine($"Retrieved from {options.OpenApiUrl}");
        var retrievedJson = options.OpenApiUrl.ToLower().EndsWith(".yaml")
            ? ConvertToJson(retrievedText)
            : EnrichJson(retrievedText);
        
        var document = await OpenApiDocument.FromJsonAsync(retrievedJson);
        Console.WriteLine($"Title: {document.Info.Title}, Version: {document.Info.Version}.");
        
        var clientGenerator = new CSharpClientGenerator(document, options.ClientGeneratorSettings);

        //var controllerGenerator = new CSharpControllerGenerator(document, options.ServiceGeneratorSettings);

        var clientCode = clientGenerator.GenerateFile(ClientGeneratorOutputType.Full);
        //var serviceCode = controllerGenerator.GenerateFile(ClientGeneratorOutputType.Full);
        
        var usings = new List<string>
        {
            "System.Collections.Generic",
            "System.CodeDom.Compiler",
            "System.Text.Json.Serialization",
            "System.Text.Json",
            "System.Runtime.Serialization",
            "System.Collections.ObjectModel",
            "System.Threading.Tasks",
            "System.Threading",
            "System.ComponentModel.DataAnnotations",
        };

        clientCode = clientCode
            .Replace(
                "<PostenLogisticsOption> FixedOptions",
                "<LogisticsOptionBase> FixedOptions"

            );
        
        clientCode = usings.Aggregate(clientCode, (current, u) => current.Replace($"{u}.", ""));

        clientCode = clientCode
            .Replace(
                "using System = global::System;",
                "using System;\r\n\tusing " + usings.Aggregate((s1, s2) => $"{s1};\r\n\tusing {s2}") + ";"
            )
            .Replace("public Customer2 Customer", "public Customer Customer")
            .Replace("public Campaign Campaign", "public CampaignResponseV3 Campaign")
            .Replace("public Pricing Pricing", "public PricingResponse Pricing");

        //Console.WriteLine($"Generated code from {options.OpenApiUrl}");
        await File.WriteAllTextAsync(options.RelativeFilePathClient, clientCode);
        Console.WriteLine($"Wrote file {options.RelativeFilePathClient}");

        //await File.WriteAllTextAsync(options.RelativeFilePathService, serviceCode);
        //Console.WriteLine($"Wrote file {options.RelativeFilePathService}");
    }

    private static string ConvertToJson(string yaml)
    {
        var deserializer = new YamlDotNet.Serialization.Deserializer();
        dynamic deserializedObject = deserializer.Deserialize<ExpandoObject>(yaml);
        string json = JsonSerializer.Serialize(deserializedObject);
        return json;
    }

    private static string EnrichJson(string jsonInput)
    {
        var deserializedObject = JsonSerializer.Deserialize<ExpandoObject>(jsonInput);
        string json = JsonSerializer.Serialize(deserializedObject);
        return json;
    }
}
