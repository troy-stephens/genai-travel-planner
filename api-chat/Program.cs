
using api_chat.Interfaces;
using api_chat.Plugins;
using api_chat.Services;
using api_chat.Util;
using api_chat.Helpers;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

//
string _apiDeploymentName = Helper.GetEnvironmentVariable("ApiDeploymentName");
string _apiEndpoint = Helper.GetEnvironmentVariable("ApiEndpoint");
string _apiKey = Helper.GetEnvironmentVariable("ApiKey");

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<Kernel>(s =>
        {
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                _apiDeploymentName,
                _apiEndpoint,
                _apiKey
                );
            
            // We are not using the AzureAISearchPlugin Search so I am commenting this out
            //builder.Services.AddSingleton<SearchIndexClient>(s =>
            //{
            //    string endpoint = _apiAISearchEndpoint;
            //    string apiKey = _apiAISearchKey;
            //    return new SearchIndexClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            //});

            // Add Singleton for AzureAISearch 
            //builder.Services.AddSingleton<IAzureAISearchService, AzureAISearchService>();

            //#pragma warning disable SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //builder.AddAzureOpenAITextEmbeddingGeneration(_textEmbeddingName, _apiEndpoint, _apiKey);
            //#pragma warning restore SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // builder.Plugins.AddFromType<DBQueryPlugin>();  
            // builder.Plugins.AddFromType<UniswapV3SubgraphPlugin>();
            // builder.Plugins.AddFromType<AzureAISearchPlugin>();

            return builder.Build();
        });
        services.AddSingleton<UserSession>();
        services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
        const string systemmsg = @$"You are a friendly assistant that specializes in helping users build and create travel plans.   
                                    You should only answer in the context of what the user is asking for.
                                    You are also responsible for fetching details about a destination and helping to plan activities along the trip.
                                    Users may ask you to make recommendations for their next trips.  
                                    When a user asks for trip recommendations make sure you asks them for the starting location, duration of the trip and where their interests are before providing recommendations.
                                    Users may ask for recommendations to a specific location.  Always ask for the startring location and duration of the trip and their interests. 
                                    Do not answer any questions that are not related to travel.
                                    If the question is not travel related, simply respond and say [I am sorry, I can only help you with travel related requests.]
                                    Summarize the provided data so it's easy to read and understand.  
                                    Do not answer any questions that are not travel related";
        services.AddSingleton<ChatHistory>(s =>
        {
            var chathistory = new ChatHistory();
            chathistory.AddSystemMessage(systemmsg);
            return chathistory;
        });

    })
    .Build();

host.Run();

