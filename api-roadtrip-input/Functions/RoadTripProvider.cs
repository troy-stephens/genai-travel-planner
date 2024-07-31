using api_roadtrip_input.Models;
using api_roadtrip_input.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net;
using System.Text.Json;

namespace api_roadtrip_input.Functions
{
    public class RoadTripProvider
    {
        private readonly ILogger<RoadTripProvider> _logger;
        private readonly Kernel _kernel;
        private bool _failedTokenLimit = false;
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _chatHistory;
        private readonly string _aiSearchIndex = Helper.GetEnvironmentVariable("AISearchIndex");
        private readonly string _semanticSearchConfigName = Helper.GetEnvironmentVariable("AISearchSemanticConfigName");


        public RoadTripProvider(ILogger<RoadTripProvider> logger, Kernel kernel, IChatCompletionService chat, ChatHistory chatHistory)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistory = chatHistory;
        }

        [Function("roadtrip")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            /*
                {
                    "status_message" : "Your request has exceeded the maximum token size for the API, please reduce the size of your payload!"
                    "token_count" : "1000" ,
                    "token_limit" : "<this would be value from environment>"
                }
            */
           
            _logger.LogInformation("C# HTTP roadtrip trigger function processed a request.");

            // Step # 1 - Implement Checks on the Data.
            // # 1 - Verify that the size of the payload does not exceed the max size limit we allow.
            // All these checks should be implemented in some sort of helper.
            var _tokenlimit = 0;
            int.TryParse(Helper.GetEnvironmentVariable("TokenLimit"), out _tokenlimit);
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var tokencount = PayloadHelper.GetTokens(requestBody);

            TokenLimitResponse tokenLimitResponse = new TokenLimitResponse();
            if (tokencount > _tokenlimit)   // max limit
            {
                tokenLimitResponse.Passed_Token_Limit = false;
                tokenLimitResponse.Status_Message = "Your request has exceeded the maximum token size for the API, please reduce the size of your payload!";
                tokenLimitResponse.Token_Limit = _tokenlimit.ToString();
                tokenLimitResponse.Token_Count = tokencount.ToString();
            }
            else
            {
                tokenLimitResponse.Passed_Token_Limit = true;
                tokenLimitResponse.Status_Message = "Your request is within the  maximum token size for the API, and is being processed.";
                tokenLimitResponse.Token_Limit = _tokenlimit.ToString();
                tokenLimitResponse.Token_Count = tokencount.ToString();
            }
            // If the size check fails we need to return a 400 Invalid Request status code to Client along with the TokenLimitReponse in the Response Boday
           
            if (requestBody == null) 
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }


            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                await response.WriteAsJsonAsync<TokenLimitResponse>(tokenLimitResponse);
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
