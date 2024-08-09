using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace api_chat.Util
{
    internal class SuggestDestinations
    {
        private string _promptCheckForDetails = @"UserId: {{$userid}}
        Question: {{$query}}
        Check the query to see if starting details were provide and populate the JSON structure below with the details:
        {
           'detailsProvided' : true,
           'startingPoint' : '<The city or address the user is starting from.>',
           'destination' : '<The destination or location user is traveling to.>,
           'duration' : '<The duration of the trip in days, weeks or months>',
           'activities' : '<The activities the user is interested in>
        }";

        private string _promptAskForMoreDetails = @"UserId: {{$userid}}
            Quesiton: {{$query}} 
            Check the question for the following details.
            Starting Point:  If the user did not provide a Starting Point, ask for the starting point, it can be a city or address.
            Duration:   If the user did not provide the duration of the trip, ask for the duration i.e. 1 day 2 days etc.
            Activities: If the user did not provide activities ask what activities are they instrested in i.e. outdoor activities, site seeing etc.

            Response:  Where will you be starting your trip from?  How long are you planning to travel?  What activities are you interested in?  

            Example: 1200 Central Ave. Charlotte, NC 28204.  5 Days and I like sight seeting, hiking, fly fishing, mountain biking.";

        private string _promptExtractStartingDetails = @"Starting Details: {{$query}}
            StartingPoint:  Extract the starting poing from the query,t can be a city or address.
            Duration: Extract the duration of the trip, it can be in days, weeks or months.
            Activities: Extract their activities i.e. outdoor activities, site seeing etc.
            The response should be in the following JSON structure:

            {
              'StartingPoint' : '<The Starting City or Address>',
              'Duration' : '<The duration of the trip>',
              'Activities' : '<The activities the user is interested in>'
            }";

        private string _promptSuggestDestinationDetails = @"Starting Details: {{$query}}
            Based on the Starting Details, suggest the following:
            Suggest 5 Destinations, and activities for the trip and also include the estimated drive time from the starting point.
            The response should be in the following JSON structure:

            {
              'destinations': [
                {
                  'name': 'Asheville, North Carolina',
                  'estimated_drive_time': '2 hours',
                  'activities': {
                    'mountain_biking': 'Ride the extensive trail network in Pisgah National Forest, including popular trails like Bent Creek and Dupont State Forest.',
                    'hiking': 'Explore the Blue Ridge Parkway and Great Smoky Mountains National Park for numerous hiking options with stunning mountain views.',
                    'fly_fishing': 'The Davidson River is a top fly fishing spot, known for its large population of trout.'
                  }
                },
                {
                  'name': 'Boone, North Carolina',
                  'estimated_drive_time': '2.5 hours',
                  'activities': {
                    'mountain_biking': 'Ride the trails at Rocky Knob Mountain Bike Park, designed for various skill levels.',
                    'hiking': 'Hike along the Appalachian Trail or explore Grandfather Mountain for challenging trails and scenic vistas.',
                    'fly_fishing': 'The Watauga River and nearby streams offer excellent fly fishing for trout.'
                  }
                }
              ]
            }";

        private string _promptSummarizeRecommendations = @"Starting Details: {{$query}}
            Based on the starting details, provide recommendations for up to 5 destinations using the following format:

            # Ashiville, North Carolina
            ## Estimated Drive Time: ~ 2 hours
            ## Activities:
            - **Mountain Biking:** details about the mountain biking
            - **Hiking:** details about the hiking
            - **Fly Fishing:** details about the fly fishing
            **Summary:** short summary of the recommendations";

        private string _promptProvideRecommendations = @"
            Starting Point: {{$startingpoint}}
            Duration: {{$duration}}
            Activities: {{$activities}}
            Based on the starting details, provide recommendations for up to 5 destinations and 3 places for accommodations using the following format:

            # Ashiville, North Carolina
            ## Estimated Drive Time: ~ 2 hours
            ## Activities:
            - **Mountain Biking:** details about the mountain biking
            - **Hiking:** details about the hiking
            - **Fly Fishing:** details about the fly fishing
            ## Accommodations:
            1. **Blackberry Farm:** details about the place
               -**Address:** address of the place
            2. **The Lodge at Buckberry Creek:** details about the place
               -**Address:** address of the place
            3. **Townsend Gateway Inn:** details about the place
               -**Address:** address of the place

            **Summary:** short summary of the recommendations";

        private string _promptProvideRecommendationsForKnownDestination = @"
            Starting Point: {{$startingpoint}}
            Duration: {{$duration}}
            Destination: {{$destination}}
            Activities: {{$activities}}
            Based on the starting details [Starting Point], provide recommendations for the destination provided and 3 places for accommodations using the following format provided in the example below:

            [Example]
            # Ashiville, North Carolina
            ## Estimated Drive Time: ~ 2 hours
            ## Activities:
            - **Mountain Biking:** details about the mountain biking
            - **Hiking:** details about the hiking
            - **Fly Fishing:** details about the fly fishing
            ## Accommodations:
            1. **Blackberry Farm:** details about the place
               -**Address:** address of the place
            2. **The Lodge at Buckberry Creek:** details about the place
               -**Address:** address of the place
            3. **Townsend Gateway Inn:** details about the place
               -**Address:** address of the place
            [End Example]
            **Summary:** short summary of the recommendations";

        private string _promptProvideRoutePlan = @"
            Query: {{$startingpoint}}
            Duration: {{$duration}}
            Activities: {{$activities}}
            Based on the starting details, provide recommendations for up to 5 destinations and 3 places for accommodations using the following format:

            # Ashiville, North Carolina
            ## Estimated Drive Time: ~ 2 hours
            ## Activities:
            - **Mountain Biking:** details about the mountain biking
            - **Hiking:** details about the hiking
            - **Fly Fishing:** details about the fly fishing
            ## Accommodations:
            1. **Blackberry Farm:** details about the place
               -**Address:** address of the place
            2. **The Lodge at Buckberry Creek:** details about the place
               -**Address:** address of the place
            3. **Townsend Gateway Inn:** details about the place
               -**Address:** address of the place

            **Summary:** short summary of the recommendations";

        public async Task<string> CheckForDetailsAsync(Kernel kernel, string userid, string query)
        {   
#pragma warning disable SKEXP0013

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object",
                MaxTokens = 400, 
            };


            KernelArguments arguments = new(executionSettings) { { "query", query }, { "userid", userid } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                Console.WriteLine("SK ,- CheckVerificationIntent");
                var response = await kernel.InvokePromptAsync(_promptCheckForDetails, arguments);
                // this code is here for debugging purposes
                    var metadata = response.Metadata;
                    Console.WriteLine($@"Starting details :{userid}");
                    Console.WriteLine(response);
                    Console.WriteLine("----------------------");
                    if (metadata != null && metadata.ContainsKey("Usage"))
                    {
                        var usage = (CompletionsUsage?)metadata["Usage"];
                        Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                    }
                // above code is not really needed
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public async Task<string> ProvidRecommendationsAsync(Kernel kernel, string userid, string startingpoint, string duration, string activities, string intent = "suggestdestinations")
        {
#pragma warning disable SKEXP0013

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                MaxTokens = 2000,
            };

            KernelArguments arguments = new(executionSettings) { { "startingpoint", startingpoint }, { "duration", duration }, { "activities", activities } };
            string result = "";
            try
            {
                string promptToUse = _promptProvideRecommendations;

                if (intent == "knowndestination")
                {
                    promptToUse = _promptProvideRecommendationsForKnownDestination;
                }
                Console.WriteLine("SK ,- ProvidRecommendationsAsync");
                var response = await kernel.InvokePromptAsync(promptToUse, arguments);
                // this code is here for debugging purposes
                    var metadata = response.Metadata;
                    Console.WriteLine($@"Provide Recommendations :{userid}");
                    Console.WriteLine(response);
                    Console.WriteLine("----------------------");
                    if (metadata != null && metadata.ContainsKey("Usage"))
                    {
                        var usage = (CompletionsUsage?)metadata["Usage"];
                        Console.WriteLine($"Token usage. Input tokens: {usage?.PromptTokens}; Output tokens: {usage?.CompletionTokens}");
                    }
                // above code is not really needed
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
