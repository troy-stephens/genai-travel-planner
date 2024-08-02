using api_roadtrip_test.Models.RoadTripDoc;
using api_roadtrip_test.Util;
using System;
using System;
using System.IO;
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

            if (roadTripDocument?.RoadTrip?.PointsOfInterest != null)
            {
                foreach (PointOfInterest poi in roadTripDocument.RoadTrip.PointsOfInterest)
                {
                    if (poi?.Id != null)
                    {
                        poiDictionary.Add(poi.Id, poi);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("RoadTrip or PointsOfInterest is null.");
            }

            var output = await localSearchService.getSearchData(poiDictionary);

            // write the output to the console in tabular format
            Console.WriteLine(output);
        }
    }
}
