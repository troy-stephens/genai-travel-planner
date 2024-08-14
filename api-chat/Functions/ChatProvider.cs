using api_chat.Models;
using api_chat.Util;
using api_chat.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net;
using System.Text.Json;

namespace api_chat.Functions
{
    public class ChatProvider
    {
        private readonly ILogger<ChatProvider> _logger;
        private readonly Kernel _kernel;
        private UserSession _userSession;
        public bool startingdetailsprovided = false;
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _chatHistory;
        private readonly string _aiSearchIndex = Helper.GetEnvironmentVariable("AISearchIndex");
        private readonly string _semanticSearchConfigName = Helper.GetEnvironmentVariable("AISearchSemanticConfigName");



        public ChatProvider(ILogger<ChatProvider> logger, Kernel kernel, IChatCompletionService chat, ChatHistory chatHistory, UserSession userSession)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistory = chatHistory;
            _userSession = userSession;
        }

        [Function("ChatProvider")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            /*
                {
                    "userId": "stevesmith@contoso.com",
                    "sessionId": "12345678",
                    "prompt": "Hello, What can you do for me?"
                }
            */
            string result = string.Empty;
            _logger.LogInformation("C# HTTP Chat API has been called");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var chatRequest = JsonSerializer.Deserialize<ChatProviderRequest>(requestBody);
            if (chatRequest == null || chatRequest.userId == null || chatRequest.sessionId == null || chatRequest.tenantId == null || chatRequest.prompt == null)
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }
            else if (_userSession != null)
            {
                
                // We could also load the userId and session details from backend by looking up these details from the backend.
                // basically retreiving the chatHistory (memory for the user and session) 
                _userSession.UserID = chatRequest.userId;
                _userSession.SessionID = chatRequest.sessionId;
                
            }
            var intent = await Util.Intent.GetIntent(_chat, chatRequest.prompt);
            // The purpose of using an Intent pattern is to allow you to make decisions about how you want to invoke the LLM
            // In the case of RAG, if you can detect the user intent is to related to searching documents, then you can only perform that action when the intent is to search documents
            // this allows you to reduce the token usage and save you TPM and dollars
            SuggestDestinations suggestDestinations = new SuggestDestinations();
            var checkfordetailsresult = await suggestDestinations.CheckForDetailsAsync(_kernel, chatRequest.userId, chatRequest.prompt);
            CheckForDetails? checkfordetails = JsonSerializer.Deserialize<CheckForDetails>(checkfordetailsresult);

            switch (intent.ToLower())
            {
                case "suggestdestinations":
                    {
                        Console.WriteLine("Intent: SuggestDestinations");
                        if (checkfordetails != null && checkfordetails.DetailsProvided)
                        {
                            // we have the details so not we can get suggestions
                            var destinationRecommendations = await suggestDestinations.ProvideRecommendationsAsync(_kernel, _userSession.UserID, checkfordetails.StartingPoint, checkfordetails.Duration, checkfordetails.Activities);
                            var userMessage = $@"Starting at: {checkfordetails.StartingPoint}, make suggestions for a {checkfordetails.Duration} trip, and the activities we interested in are: {checkfordetails.Activities}";
                            _chatHistory.AddUserMessage(userMessage);
                            _chatHistory.AddAssistantMessage(destinationRecommendations);
                            result = destinationRecommendations;
                            _userSession.StartingDetailsProvided = true;
                            // return the response
                        }
                        else
                        {
                            // starting point, duration and activities not provided need to ask the user to provide those details.
                            result = Constants.AskForStartingDetails;
                        }

                        break;
                    }
                case "suggestactivities":
                    {
                        // Now that I know the intent of the question is graphql related, I could just call the plugin directly
                        // but, since I have AutoInvokeKernelFunctions enabled I can just let SK detect that it needs to call the funciton and let it do it.
                        // Now, it would be more performant to just call it directly as their is additional overhead with SK searching the plugin collection etc
                        Console.WriteLine("Intent: accommodations");
                        if (!_userSession.StartingDetailsProvided)
                        {
                            result = Constants.AskForStartingDetails;
                        }
                        else
                        {
                            // We are going to let the LLM generate the RoutePlan, no need for a special prompt here unless we what specific formatting, then we can use the approach we used above
                            _chatHistory.AddUserMessage(chatRequest.prompt);
                            var resultChatCompletion = await _chat.GetChatMessageContentAsync(
                                _chatHistory,
                                executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.7, TopP = 0.0 },
                                kernel: _kernel);
                            _chatHistory.AddAssistantMessage(resultChatCompletion?.Content ?? "");
                            result = resultChatCompletion?.Content ?? "";
                        }
                        break;
                    }
                case "accommodations":
                    {
                        Console.WriteLine("Intent: accommodations");
                        if (!_userSession.StartingDetailsProvided)
                        {
                            result = Constants.AskForStartingDetails;
                        }
                        else
                        {
                            // We are going to let the LLM generate the RoutePlan, no need for a special prompt here unless we what specific formatting, then we can use the approach we used above
                            _chatHistory.AddUserMessage(chatRequest.prompt);
                            var resultChatCompletion = await _chat.GetChatMessageContentAsync(
                                _chatHistory,
                                executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.7, TopP = 0.0 },
                                kernel: _kernel);
                            _chatHistory.AddAssistantMessage(resultChatCompletion?.Content ?? "");
                            result = resultChatCompletion?.Content ?? "";
                        }

                        break;
                    }
                case "knowndestination":
                    {
                        Console.WriteLine("Intent: KnownDestination");
                        if (checkfordetails != null && checkfordetails.DetailsProvided)
                        {
                            // we have the details so not we can get suggestions
                            var destinationRecommendations = await suggestDestinations.ProvideRecommendationsAsync(_kernel, chatRequest.userId, checkfordetails.StartingPoint, checkfordetails.Duration, checkfordetails.Activities, "knowndestination");
                            var userMessage = $@"Starting at: {checkfordetails.StartingPoint}, make suggestions for a {checkfordetails.Duration} trip, and the activities we interested in are: {checkfordetails.Activities}";
                            _chatHistory.AddUserMessage(userMessage);
                            _chatHistory.AddAssistantMessage(destinationRecommendations);
                            result = destinationRecommendations;
                            _userSession.StartingDetailsProvided = true;
                            // return the response
                        }
                        else
                        {
                            // starting point, duration and activities not provided need to ask the user to provide those details.
                            result = Constants.AskForStartingDetails;
                        }
                        break;
                    }
                case "tellmemore":
                    {
                        Console.WriteLine("Intent: tellmemore");
                        if (!_userSession.StartingDetailsProvided)
                        {
                            result = Constants.AskForStartingDetails;
                        }
                        else
                        {
                            // We are going to let the LLM generate the RoutePlan, no need for a special prompt here unless we what specific formatting, then we can use the approach we used above
                            _chatHistory.AddUserMessage(chatRequest.prompt);
                            var resultChatCompletion = await _chat.GetChatMessageContentAsync(
                                _chatHistory,
                                executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.7, TopP = 0.0 },
                                kernel: _kernel);
                            _chatHistory.AddAssistantMessage(resultChatCompletion?.Content ?? "");
                            result = resultChatCompletion?.Content ?? "";
                        }
                        break;
                    }
                case "routeplan":
                    {
                        Console.WriteLine("Intent: routeplan");
                        if (!_userSession.StartingDetailsProvided)
                        {
                            result = Constants.AskForStartingDetails;
                        }
                        else
                        {
                            // We are going to let the LLM generate the RoutePlan, no need for a special prompt here unless we what specific formatting, then we can use the approach we used above
                            _chatHistory.AddUserMessage(chatRequest.prompt);
                            var resultChatCompletion = await _chat.GetChatMessageContentAsync(
                                _chatHistory,
                                executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.7, TopP = 0.0 },
                                kernel: _kernel);
                            _chatHistory.AddAssistantMessage(resultChatCompletion?.Content ?? "");
                            result = resultChatCompletion?.Content ?? "";
                        }

                        break;
                    }
                case "unknown":
                    {
                        Console.WriteLine("Intent: Unknown");
                        _chatHistory.AddUserMessage("Simply respond in a polite way that the request is not related to travel so you are unable to help.");
                        result = Constants.ICanHelpWith;
                        break;
                    }

            }

            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                var unknown = @$"I can help you with travel planning, so please make sure your questions are travel related";
                await response.WriteStringAsync(result ?? unknown);
            }
            catch (Exception ex)
            {
                // Log exception details here
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to propagate it further
            }

            return response;
            }
        }
}
