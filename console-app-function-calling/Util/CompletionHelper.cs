using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using System.Text.Json;


namespace console_app_function_calling.Util
{
    internal static class CompletionHelper
    {
        public static async Task<string> GetCompletionAsync(Kernel kernel, ChatHistory history, string prompt)
        {
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions //ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await chatCompletionService.GetChatMessageContentAsync(
                           history,
                           executionSettings: openAIPromptExecutionSettings,
                           kernel: kernel);
            #pragma warning disable SKEXP0001
            var functions = FunctionCallContent.GetFunctionCalls(result);
            Console.WriteLine($"Ask: {prompt}");

            if (functions.Count() == 0)
            {
                Console.WriteLine($"LLM did not find Plugin or Function call for this request.");
            }

            foreach (var function in functions)
            {
                Console.WriteLine($"Plugin: {function.PluginName} , FunctionName: {function.FunctionName}");
            }
            return result.ToString() ?? "";
        }
    }
}
