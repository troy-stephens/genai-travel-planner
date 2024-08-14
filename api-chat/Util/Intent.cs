using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Security.Principal;
using System.Text.RegularExpressions;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;


namespace api_chat.Util
{
    internal static class Intent
    {
        // This function is a very powerful Intent helper, it allows you to detect the intent and take action accordingly which 
        // is much more efficient, reduces the number of tokens consumed and allows you to avoid unnecesscary calls to your AI search or the LLM
        // I have seen other attempts at this, but this approach is very powerful and you are letting the LLM attempt to detect the Intent 3 times
        // we then take the quorum of the 3 results and use that for the intent
        public static async Task<string> GetIntent(IChatCompletionService chat, string query)
        {
            // Keep the ChatHistory local since we only need it to detect the Intent
            ChatHistory chatHistory = new ChatHistory();
            var whatistheintent = "not_travel"; // default
            chatHistory.AddSystemMessage($@"Identify the user's intent. Return one of the following values:

            SuggestDestinations - If the user wants destination recommendations
            SuggestActivities - If the user wants recommendations for activities at a given destination
            Accommodations - If the user wants recommendations for accommodations
            KnownDestination - If the user knows the destination
            TellMeMore - If the user wants more information
            Unknown - If the user's intent matches none of the above

            Examples:
            User: I’m planning a road trip but don’t know where to start. Can you help me plan it?
            Intent: SuggestDestinations
            User: I want to go on vacation, but I’m not sure where to go. Can you suggest some destinations?
            Intent: SuggestDestinations

            Examples:
            User: Please suggest some activities, sites to see, places to visit that would make our trip more interesting
            Intent: SuggestActivities
            User: I want to go on vacation, but I’m not sure where to go. Can you suggest some destinations?
            Intent: SuggestActivities

            User: I would like to plan a trip to Colorado in December.  Can you provide some recommendations for 7 day trip?
            Intent: KnownDestination
            User: we will like to take a two day trip to Charleston, SC.  Can you provide some recommendations?
            Intent: KnownDestination 
                    
            User: Tell me more about the sites in the area.
            Intent: tell_me_more
            User: Tell me about the restaurants.
            Intent: tell_me_more 

            User: What is the top performing stock in 2020?
            Intent: Unknown
            User question: Why is the Sky Blue?
            Intent: Unknown
            User question: What is 10x10?
            Intent: Unknown");


            chatHistory.AddUserMessage("I am not sure where to go for a summer trip.");
            chatHistory.AddAssistantMessage("SuggestDestinations");
            chatHistory.AddUserMessage("Can you help me plan a trip to Colorado?");
            chatHistory.AddAssistantMessage("KnownDestination");
            chatHistory.AddUserMessage("Can you recommend accommodations");
            chatHistory.AddAssistantMessage("Accommodations");
            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                ResultsPerPrompt = 3, // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
            };
            try
            {
                // Call the ChatCompletion asking for 3 rounds to attempt to identify that intent
                var result = await chat.GetChatMessageContentsAsync(
                        chatHistory,
                        executionSettings);

                string threeturnresult = string.Join(", ", result.Select(o => o.ToString()));
                // Now we use Regex and Linq to find the intent that is repeated the most
                var words = Regex.Split(threeturnresult.ToLower(), @"\W+")
                      .Where(w => w.Length >= 3)
                      .GroupBy(w => w)
                      .OrderByDescending(g => g.Count())
                      .First();
                whatistheintent = words.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return whatistheintent;
        }
    }
}