using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;
using api_roadtrip_input.Models;
using api_roadtrip_input.Util;
using Azure.AI.OpenAI;
using System.Text.Json;
using Microsoft.ML.Tokenizers;


namespace api_roadtrip_input.Util 
{
    internal static class PayloadHelper
    {
        public static async Task<string> CheckForHarmfulDataAsync(Kernel kernel, string vendor, RoadTripRoot payload)
        {
            string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var _promptHarmfulCheck = PromptHelper._promptHarmfulCheck; 
            
            #pragma warning disable SKEXP0010
            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
            };
            HarmfulCheckResult? harmfulcheckresult = null;
            KernelArguments arguments = new(executionSettings) { { "vendor", vendor }, { "date", DateTime.Now.ToString("MM/dd/yyy") }, { "payload", jsonPayload } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                Console.WriteLine("SK ,- GetHarmCheckResult");
                var response = await kernel.InvokePromptAsync(_promptHarmfulCheck, arguments);
                var metadata = response.Metadata;
                Console.WriteLine(response);
                Console.WriteLine("----------------------");
                harmfulcheckresult = JsonSerializer.Deserialize<HarmfulCheckResult>(response.GetValue<string>() ?? ""); 

                if (metadata != null && metadata.ContainsKey("Usage"))
                {
                    var usage = (CompletionsUsage?)metadata["Usage"];
                    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                }
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public static async Task<string> CheckForHtmlContentAsync(Kernel kernel, string payload)
        {
 
            var _promptCheckForHtml = PromptHelper._promptCheckForHtml;

            #pragma warning disable SKEXP0010
            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
            };
            KernelArguments arguments = new(executionSettings) { { "payload", payload } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                Console.WriteLine("SK ,- GetHarmCheckResult");
                var response = await kernel.InvokePromptAsync(_promptCheckForHtml, arguments);
                var metadata = response.Metadata;
                Console.WriteLine(response);
                Console.WriteLine("----------------------");
               
                if (metadata != null && metadata.ContainsKey("Usage"))
                {
                    var usage = (CompletionsUsage?)metadata["Usage"];
                    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                }
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public static async Task<string> ConvertUnstructuredToRoadtripAsync(Kernel kernel, string payload)
        {
 
            var _promptConvertUnstructeredToRoadTrip = PromptHelper._promptConvertUnstructredToRoadTrip;

            #pragma warning disable SKEXP0010
            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
            };
            KernelArguments arguments = new(executionSettings) { { "payload", payload } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                Console.WriteLine("SK ,- ConvertUnstructedToRoadtrip");
                var response = await kernel.InvokePromptAsync(_promptConvertUnstructeredToRoadTrip, arguments);
                var metadata = response.Metadata;
                Console.WriteLine(response);
                Console.WriteLine("----------------------");

                if (metadata != null && metadata.ContainsKey("Usage"))
                {
                    var usage = (CompletionsUsage?)metadata["Usage"];
                    Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                }
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public static int GetTokens(string payload)
        {
            // Must install the preview version of the library to get the CreateTiktokenForModel function
            // dotnet add package Microsoft.ML.Tokenizers --version 0.22.0-preview.24271.1
            // gpt-4 or gpt-3.5-turbo 4o has not been added to the dictionary yet but this serves the purpose

            Tokenizer _tokenizer = Tokenizer.CreateTiktokenForModel("gpt-4");

            return _tokenizer.CountTokens(payload);
        }

    }
}