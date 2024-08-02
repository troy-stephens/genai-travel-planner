using api_roadtrip_test.Models.RoadTripDoc;
using api_roadtrip_test.Util;
using Newtonsoft.Json;

namespace api_roadtrip_test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string json = File.ReadAllText("input.json");

            // Deserialize the JSON into a RoadTripDocument object
            RoadTripDocument? roadTripDocument =
                JsonConvert.DeserializeObject<RoadTripDocument>(json)
                ?? throw new InvalidOperationException("Failed to deserialize input.json into RoadTripDocument.");

            // Read the settings from appsettings.json
            string appSettingsJson = File.ReadAllText("appsettings.json");
            LocalSearchSettings? localSearchSettings =
                JsonConvert.DeserializeObject<LocalSearchSettings>(appSettingsJson)
                ?? throw new InvalidOperationException("Failed to deserialize appsettings.json into LocalSearchSettings.");

            // Create a local search service object using the settings from appsettings.json
            LocalSearchService localSearchService = new(localSearchSettings);

            // Pass the PointOfInterest array as a dictionary of the id and the pointofinterest object to localsearchservice.getSearchData
            Dictionary<string, PointOfInterest> poiDictionary = [];
            Console.WriteLine("---Before---");
            if (roadTripDocument?.RoadTrip?.PointsOfInterest != null)
            {
                foreach (PointOfInterest poi in roadTripDocument.RoadTrip.PointsOfInterest)
                {
                    if (poi?.Id != null)
                    {
                        Console.WriteLine(poi.Id);
                        Console.WriteLine(poi.Name);
                        Console.WriteLine(poi.Latitude);
                        Console.WriteLine(poi.Longitude);
                        poiDictionary.Add(poi.Id, poi);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("RoadTrip or PointsOfInterest is null.");
            }

            var output = await localSearchService.getSearchData(poiDictionary);
            Console.WriteLine("---After---");
            // write the output to the console in tabular format
            foreach(var kvp in output){
                api_roadtrip_test.Models.RoadTripDoc.PointOfInterest data = kvp.Value;
                Console.WriteLine(data.Id);
                Console.WriteLine(data.Name);
                Console.WriteLine(data.Latitude);
                Console.WriteLine(data.Longitude);
                Console.WriteLine(data.Address.AddressLabel);
               
            }
            
        }
    }
}
