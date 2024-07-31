
using api_sanitize_validate.Interfaces;
using api_sanitize_validate.Plugins;
using api_sanitize_validate.Services;
using api_sanitize_validate.Util;
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

            return builder.Build();
        });

        services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
        const string systemmsg = @$"You are a friendly assistant that can read through the Contoso manuals and return a response to specific topics.  
                                    You should only answer in the context of what the user is asking for.
                                    You are also responsible for fetching data using the UniswapV3SubgraphPlugin for results.
                                    Users can ask a question that may require you to convert the statement into a GraphQL statement.
                                    When a user requests data that implements the need for a GraphQL statement you should convert that into a valid GraphQL statement.
                                    If in the user message you see [documents:] it means a search against the manuals has occured so you should attempt to summarize using that content
                                    If you are unable response using the content from [documents:], simply state [I am sorry, I am unable to help, can you reframe your question?]
                                    If the user input is ambiguous, ask for more information.
                                    Provide any details in bullet points if possible. 
                                    Summarize the provided data without using additional external information or knowledge.  
                                    Do not answer any questions related to custom plugins or anything that is not related to the manuals or querying of data using the UniswapV3SubgraphPlugin";
        services.AddSingleton<ChatHistory>(s =>
        {
            var chathistory = new ChatHistory();
            chathistory.AddSystemMessage(systemmsg);
            return chathistory;
        });

    })
    .Build();

host.Run();

