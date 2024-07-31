using api_sanitize_validate.Models;
using api_sanitize_validate.Util;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net;
using System.Text.Json;

namespace api_sanitize_validate.Functions
{
    public class SanitizeValidator
    {
        private readonly ILogger<SanitizeValidator> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _chatHistory;


        public SanitizeValidator(ILogger<SanitizeValidator> logger, Kernel kernel, IChatCompletionService chat, ChatHistory chatHistory)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistory = chatHistory;
            // we are not using chat history so we need to come back and clean this up.
        }

        [Function("harm-checks-struct")]
        public async Task<HttpResponseData> RunStruct([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            // See the example payloads in the Data folder
            _logger.LogInformation("C# HTTP SentimentAnalysis trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var tripRequest = JsonSerializer.Deserialize<RoadTripRoot>(requestBody);
            if (tripRequest == null || tripRequest.Vendor == null || tripRequest.RoadTrip == null || tripRequest.RoadTrip.Start == null || tripRequest.RoadTrip.End== null )
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }
            var result = await PayloadHelper.CheckForHarmfulDataAsync(_kernel, "Contoso XYZ", tripRequest);

            var harmfulcheckresult = JsonSerializer.Deserialize<HarmfulCheckResult>(result);


            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(result);

            }
            catch (Exception ex)
            {
                // Log exception details here
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to propagate it further
            }

            return response;
        }

        [Function("unstructred-convert")]
        public async Task<HttpResponseData> RunUnstruct([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            // See the example payloads in the Data folder
            _logger.LogInformation("C# HTTP POST harm-check trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (requestBody == null )
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }
            var result = await PayloadHelper.ConvertUnstructuredToRoadtripAsync(_kernel, requestBody);

            // var convertedToRoadtripResult = JsonSerializer.Deserialize<RoadTripRoot>(result);


            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(result);
            }
            catch (Exception ex)
            {
                // Log exception details here
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to propagate it further
            }

            return response;
        }

        [Function("check-for-html")]
        public async Task<HttpResponseData> RunCheckForHtml([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            // See the example payloads in the Data folder
            _logger.LogInformation("C# HTTP POST check-for-html trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (requestBody == null)
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }
            var result = await PayloadHelper.CheckForHtmlContentAsync(_kernel, requestBody);

            // var convertedToRoadtripResult = JsonSerializer.Deserialize<RoadTripRoot>(result);


            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            try
            {
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(result);
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
