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
        private const string _promptCheckForDetails = @"UserId: {{$userid}}
        Question: {{$query}}
        Check the query to see if starting details were provided and populate the JSON structure below with the details:
        {
           'detailsProvided' : true,
           'startingPoint' : '<The city or address the user is starting from.>',
           'destination' : '<The destination or location user is traveling to.>,
           'duration' : '<The duration of the trip in days, weeks or months>',
           'activities' : '<The activities the user is interested in>
        }";

        private const string _promptAskForMoreDetails = @"UserId: {{$userid}}
            Question: {{$query}} 
            Check the question for the following details.
            Starting Point:  If the user did not provide a Starting Point, ask for the starting point, it can be a city or address.
            Duration:   If the user did not provide the duration of the trip, ask for the duration i.e. 1 day 2 days etc.
            Activities: If the user did not provide activities ask what activities are they interested in i.e. outdoor activities, site seeing etc.

            Response:  Where will you be starting your trip from?  How long are you planning to travel?  What activities are you interested in?  

            Example: 1200 Central Ave. Charlotte, NC 28204.  5 Days and I like sight seeing, hiking, fly fishing, mountain biking.";

        private const string _promptExtractStartingDetails = @"Starting Details: {{$query}}
            StartingPoint:  Extract the starting point from the query. it can be a city or address.
            Duration: Extract the duration of the trip, it can be in days, weeks or months.
            Activities: Extract their activities i.e. outdoor activities, site seeing etc.
            The response should be in the following JSON structure:

            {
              'StartingPoint' : '<The Starting City or Address>',
              'Duration' : '<The duration of the trip>',
              'Activities' : '<The activities the user is interested in>'
            }";

        private const string _promptSuggestDestinationDetails = @"Starting Details: {{$query}}
            Based on the Starting Details, suggest the following:
            Suggest 5 Destinations, and activities for the trip and also include the estimated drive time from the starting point.
            The response should be in the following JSON structure:

                {
                    'message': 'short summary of the recommendation(s)'
                    'destinations': 
                    [
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                        },
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                        },
                    ]
                }";

        private const string _promptSummarizeRecommendations = @"Starting Details: {{$query}}
            Based on the starting details, provide recommendations for up to 5 destinations.
            The response should be in the following JSON structure:

                {
                    'message': 'short summary of the recommendation(s)'
                    'destinations': 
                    [
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                        },
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                        },
                    ]
                }";

            //# Asheville, North Carolina
            //## Estimated Drive Time: ~ 2 hours
            //## Activities:
            //- **Mountain Biking:** details about the mountain biking
            //- **Hiking:** details about the hiking
            //- **Fly Fishing:** details about the fly fishing
            //**Summary:** short summary of the recommendations";

        private const string _promptProvideRecommendations = @"
            Starting Point: {{$startingpoint}}
            Duration: {{$duration}}
            Activities: {{$activities}}
            Based on the starting details, provide recommendations for up to 5 destinations and 3 places for accommodations.
            The response should be in the following JSON structure:

                {
                    'message': 'short summary of the recommendation(s)'
                    'destinations': 
                    [
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                            'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],

                        },
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                            'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],
                        },
                    ]
                }";

        private const string _promptProvideRecommendationsForKnownDestination = @"
            Starting Point: {{$startingpoint}}
            Destination: {{$destination}}
            Duration: {{$duration}}
            Destination: {{$destination}}
            Activities: {{$activities}}
            Based on the starting details, provide recommendations for the destination provided and 3 places for accommodations.
            The response should be in the following JSON structure:

                {
                    'message': 'short summary of the recommendation(s)'
                    'destinations': 
                    [
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                             'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],
                       },
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                            'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],
                        },
                    ]
                }";

        private const string _promptProvideRoutePlan = @"
            Query: {{$startingpoint}}
            Duration: {{$duration}}
            Activities: {{$activities}}
            Based on the starting details, provide recommendations for up to 5 destinations and 3 places for accommodations.
            The response should be in the following JSON structure:

                {
                    'message': 'short summary of the recommendation(s)'
                    'destinations': 
                    [
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                            'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],
                        },
                        {
                            'destination': 'destination',
                            'starting_point': 'starting point',
                            'summary': 'short description of the suggested trip'
                            'estimated_drive_time': 'approximate time to drive there',
                            'activities': 
                            [
                                {
                                    'title': 'activity 1',
                                    'description': 'description of activity 1'
                                },
                                {
                                    'title': 'activity 2',
                                    'description': 'description of activity 2'
                                },
                                {
                                    'title': 'activity 3',
                                    'description': 'description of activity 3'
                                },
                            ],
                            'accommodations': 
                            [
                                {
                                    'title': 'accommodation 1',
                                    'description': 'description of accommodation 1'
                                },
                                {
                                    'title': 'accommodation 2',
                                    'description': 'description of accommodation 2'
                                },
                                {
                                    'title': 'accommodation 3',
                                    'description': 'description of accommodation 3'
                                },
                            ],
                        },
                    ]
                }";

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

        public async Task<string> ProvideRecommendationsAsync(Kernel kernel, string userid, string startingpoint, string duration, string activities, string destination = "", string intent = "suggestdestinations")
        {
#pragma warning disable SKEXP0013

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
                MaxTokens = 2000,
            };

            KernelArguments arguments = new(executionSettings) { { "startingpoint", startingpoint }, { "duration", duration }, { "activities", activities }, { "destination", destination } };
            string result = "";
            try
            {
                string promptToUse = _promptProvideRecommendations;

                if (intent == "knowndestination")
                {
                    promptToUse = _promptProvideRecommendationsForKnownDestination;
                }
                Console.WriteLine("SK ,- ProvideRecommendationsAsync");
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
