using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;
using api_sanitize_validate.Models;
using api_sanitize_validate.Util;
using Azure.AI.OpenAI;
using System.Text.Json;


namespace api_sanitize_validate.Util
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
    }
}