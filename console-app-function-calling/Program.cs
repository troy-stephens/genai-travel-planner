using Console_SK_Planner.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKTraining.Plugins;
using console_app_function_calling.Plugins;
using System;
using System.Configuration;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;
using System.Threading;
using Azure.AI.OpenAI;
using console_app_function_calling.Util;

var openAiDeployment = ConfigurationManager.AppSettings.Get("AzureOpenAIModel");
var openAiUri = ConfigurationManager.AppSettings.Get("AzureOpenAIEndpoint");
var openAiApiKey = ConfigurationManager.AppSettings.Get("AzureOpenAIKey");
var azureMapsApiKey = ConfigurationManager.AppSettings.Get("AzureMapsKey");

var exampleToRun = "AzureMaps"; // Can be either AzureMaps or SKFunctionCalling;

#region Azure Maps Routing Example
if (exampleToRun == "AzureMaps")
{
    var result = await AzureMapsHelper.GetRouteAsync(azureMapsApiKey!);
    var result2 = await AzureMapsHelper.GetGeoCodingForAddressAsync(azureMapsApiKey!);
}
    #endregion

    #region Function Calling Example
    if (exampleToRun == "SkFunctionCalling")
    {
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("SK EnableKernelFunctions Example of how LLM can select wrong plugin/function");
        var builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
           deploymentName: openAiDeployment!,
           endpoint: openAiUri!,
           apiKey: openAiApiKey!);

        builder.Plugins.AddFromType<Example2EchoPlugin>();
        builder.Plugins.AddFromType<LightOnPlugin>();
        builder.Plugins.AddFromType<WeatherPlugin>();
        builder.Plugins.AddFromType<FollowUpPlugin>();

        var kernel = builder.Build();

        // let's just add some random plugins

        kernel.Plugins.AddFromFunctions("time_plugin",
        [
            KernelFunctionFactory.CreateFromMethod(
            method: () => DateTime.Now,
            functionName: "get_time",
            description: "Get the current time"
        ),
        KernelFunctionFactory.CreateFromMethod(
            method: (DateTime start, DateTime end) => (end - start).TotalSeconds,
            functionName: "diff_time",
            description: "Get the difference between two times in seconds"
        )
        ]);

        //kernel.Plugins.AddFromFunctions("trip_planner",
        //[
        //    KernelFunctionFactory.CreateFromMethod(
        //        method: (string query) => $"Ok I can help you plan a trip to {query}",
        //        functionName: "plan_trip",
        //        description: "Help users plan trips"
        //    ),
        //    KernelFunctionFactory.CreateFromMethod(
        //        method: (string query) => $"Ok I can help you plan a trip to {query}",
        //        functionName: "modify_trip",
        //        description: "Help users modify a trip"
        //    )
        //]);

        ChatHistory history = [];

        string jsonData = @"
        {
            ""tour_id"": ""123"",
            ""place_id"": ""456"",
            ""place_name"": ""Beautiful Place"",
            ""function_name"": ""GetTourDetails"",
            ""ask"": ""Tell me more about this place""
        }";

        var examplePrompt = $"Can you please modify a trip for me, for {jsonData} ?";

        history.AddUserMessage(examplePrompt);

        var result = await CompletionHelper.GetCompletionAsync(kernel, history, examplePrompt);
        history.Clear();
        // Now lets load of the chathistory with some additional data
        history.AddUserMessage("place_id='3', place_name='Blah'");
        history.AddSystemMessage("You are a nice AI Assistant");
        // let call it again after adding some data to the chat history that is unrelated to modify trip
        var result2 = await CompletionHelper.GetCompletionAsync(kernel, history, examplePrompt);

        kernel.Plugins.Clear();
        
    }
    #endregion
