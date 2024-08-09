using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using console_app_function_calling.Model;
using console_app_function_calling.Plugins;

namespace console_app_function_calling.Plugins
{
   public sealed class FollowUpPlugin
    {
        private readonly IChatCompletionService _chatService;
       

        public FollowUpPlugin(IChatCompletionService chatService)
        {
            _chatService = chatService;
          
        }

        [Microsoft.SemanticKernel.KernelFunction, Description("Provides follow-up on suggested road trip, offering information about suggested places")]
        public async Task<string> TripFollowUp(
            [Description("Id of the place from recent interactions with the user"), Required] string placeId,
            [Description("Name of the place from recent interactions with the user"), Required] string placeName,
            [Description("User's query"), Required] string ask)
        {
            var sampledata = new TourData
            {
                TourId = "1721728437",
                PlaceId = "2",
                PlaceName = "Ratlam",
                FunctionName    = "TripFollowup",
                Ask = ask,
            };
            var jsonString = JsonSerializer.Serialize(sampledata);
            await Task.Delay(1);
            return jsonString;
        }

        [Microsoft.SemanticKernel.KernelFunction, Description("Modify road trip by specifying changes like removing specific waypoints, avoiding certain types of locations, or adding new destinations based on your interests. For example, you can say, 'Remove the second stop,' 'Avoid historical sites,' or 'Add scenic viewpoints along the route.")]
        public async Task<string> ModifyTrip(
            [Description("Tour Id from recent interactions with the user"), Required] string tourid,
            [Description("Id of the place from recent interactions with the user"), Required] string placeId, 
            [Description("Name of the place from recent interactions with the user"), Required] string placeName, 
            [Description("User's recent query"), Required] string ask) {

                Console.WriteLine("------------------------------INSIDE FOLLOWUP OF MODIFY-------------------");
                Console.WriteLine("tourid : "+ tourid);
                Console.WriteLine("placeid : "+ placeId);
                Console.WriteLine("placeName : "+ placeName);
                Console.WriteLine("user's ask : "+ ask);

            /*
            tourid : 1721728437
            placeid : 2
            placeName : Ratlam
            user's ask : Would you like me to suggest a different place to replace Ratlam?
            */

            var sampledata = new TourData
            {
                TourId = "1721728437",
                PlaceId = "2",
                PlaceName = "Ratlam",
                FunctionName = "ModifyTrip",
                Ask = ask,
            };
            var jsonString = JsonSerializer.Serialize(sampledata);
            await Task.Delay(1);
            return jsonString;

        }

        [Microsoft.SemanticKernel.KernelFunction, Description("Provide follow-up on previously suggested activities to the user, offering additional details and information like duration, price etc.")]
        public async Task<string> ActivityFollowUp(
            [Description("Id of activity from recent interactions with the user"), Required] string activityId,
            [Description("Name of activity from recent interactions with the user"), Required] string activityName,
            [Description("User's query"), Required] string ask)
        {
            var sampledata = new TourData
            {
                TourId = "1721728437",
                PlaceId = "2",
                PlaceName = "Ratlam",
                FunctionName = "ActivityFollowUp",
                Ask = ask,
            };
            var jsonString = JsonSerializer.Serialize(sampledata);
            await Task.Delay(1);
            return jsonString;
        }
    }
}
