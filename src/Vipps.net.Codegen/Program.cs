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
        var epaymentOptions = new CodegenSettings("Epayment", Path.Combine(relativeFilePath, "EpaymentModels.cs"));
        await GenerateCode(httpClient, epaymentOptions);

        //Checkout
        var checkoutOptions = new CodegenSettings("Checkout", Path.Combine(relativeFilePath, "CheckoutModels.cs"));
        await GenerateCode(httpClient, checkoutOptions);

        //Recurring
        var recurringOptions = new CodegenSettings("Recurring", Path.Combine(relativeFilePath, "RecurringModels.cs")); //https://developer.vippsmobilepay.com/api/qr/#operation/CreateMerchantRedirectQr https://developer.vippsmobilepay.com/redocusaurus/CreateMerchantRedirectQr-swagger-id.yaml
        await GenerateCode(httpClient, recurringOptions);

        //Login
        var loginOptions = new CodegenSettings("Login", Path.Combine(relativeFilePath, "LoginModels.cs"));
        await GenerateCode(httpClient, loginOptions);
    }

    private static async Task GenerateCode(HttpClient httpClient, CodegenSettings options)
    {
        Console.WriteLine($"Fetching from {options.OpenApiUrl}");
        var retrievedText = await httpClient.GetStringAsync(options.OpenApiUrl);
        Console.WriteLine($"Retrieved from {options.OpenApiUrl}");
        var retrievedJson = options.OpenApiUrl.ToLower().EndsWith(".yaml")
            ? ConvertToJson(retrievedText)
            : EnrichJson(retrievedText);
        
        var document = await OpenApiDocument.FromJsonAsync(retrievedJson);
        Console.WriteLine(
            $"Generated document from {options.OpenApiUrl}: Title: {document.Info.Title}, Version: {document.Info.Version}."
        );
        
        var generator = new CSharpClientGenerator(document, options.ClientGeneratorSettings);

        var code = generator.GenerateFile(ClientGeneratorOutputType.Contracts);
        //if (code.Contains("Customer2")) { throw new Exception(code); }
        var usings = new List<string>
        {
            "System.Collections.Generic",
            "System.CodeDom.Compiler",
            "System.Text.Json.Serialization",
            "System.Runtime.Serialization",
            "System.Collections.ObjectModel",
            //"System",
        };

        code = code
            .Replace(
                "<PostenLogisticsOption> FixedOptions",
                "<LogisticsOptionBase> FixedOptions"

            );


        code = usings.Aggregate(code, (current, u) => current.Replace($"{u}.", ""));

        code = code.Replace(
            "using System = global::System;",
            "using System;\r\n\tusing " + usings.Aggregate((s1, s2) => $"{s1};\r\n\tusing {s2}") + ";"
        );
        Console.WriteLine($"Generated code from {options.OpenApiUrl}");
        await File.WriteAllTextAsync(options.RelativeFilePath, code);
        Console.WriteLine($"Wrote file {options.RelativeFilePath}");
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
