
using Azure;
using System.Text.Json;
using Azure.Core.GeoJson;
using Azure.Maps.Routing;
using Azure.Maps.Routing.Models;
using Azure.Maps.Search;
using Azure.Maps.Search.Models;
//using Azure.Maps.Search.Models.Queries;
//using Azure.Maps.Search.Models.Options;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Maps;
using Azure.ResourceManager.Maps.Models;



//using Azure;
//using Azure.Identity;
//using System;



namespace console_app_function_calling.Util
{
    internal static class AzureMapsHelper
    {
        public static async Task<string> GetRouteAsync(string apiKey)
        {
            // Bing Maps Routing is being deprecated so everyone needs to more over to Azure Maps
            // This code is nothing special I extrated it from the following repo for a quick example
            // This just demos the routing planning capibilities but it requires a list of GeoPosition
            // The API may allow for a List of Addresses but I need to do some more digging for that.
            // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/maps/Azure.Maps.Routing/samples/RouteDirectionsSamples.md
            
            // Create a MapsRoutingClient that will authenticate through Subscription Key (Shared key)
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            MapsRoutingClient client = new MapsRoutingClient(credential);

            // Create origin and destination routing points
            List<GeoPosition> routePoints = new List<GeoPosition>()
            {
                new GeoPosition(123.751, 45.9375),
                new GeoPosition(123.791, 45.96875),
                new GeoPosition(123.767, 45.90625)
            };

            // Create Route direction query object
            RouteDirectionQuery query = new RouteDirectionQuery(routePoints);
            Response<RouteDirections> result = client.GetDirections(query);

            // Route direction result
            Console.WriteLine($"Total {0} route results", result.Value.Routes.Count);
            Console.WriteLine(result.Value.Routes[0].Summary.LengthInMeters);
            Console.WriteLine(result.Value.Routes[0].Summary.TravelTimeDuration);

            // Route points
            foreach (RouteLeg leg in result.Value.Routes[0].Legs)
            {
                Console.WriteLine("Route path:");
                foreach (GeoPosition point in leg.Points)
                {
                    Console.WriteLine($"point({point.Latitude}, {point.Longitude})");
                }
            }
            await Task.Delay(1);
            return "finished";
        }

        public static async Task<string> GetGeoCodingForAddressAsync(string apiKey)
        {
            // Create a MapsRoutingClient that will authenticate through Subscription Key (Shared key)
            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            var client = new MapsSearchClient(credential);
            
            var addressToSearch = "15171 NE 24th St, Redmond, WA 98052, United States";

            var addressResults = await client.SearchAddressAsync(addressToSearch);
            Console.WriteLine($"Address To Search:{addressToSearch}");
            Console.WriteLine("Result for query: \"{0}\"", addressResults.Value.Results[0].Position);
            await Task.Delay(1);
            return "done";
          
        }
    }
}
