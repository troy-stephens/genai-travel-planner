using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using api_roadtrip_test.Models;
using api_roadtrip_test.Models.RoadTripDoc;

namespace api_roadtrip_test.Util 
{
    public class LocalSearchSettings
    {
        public required string Endpoint { get; set; }
        public required string ApiKey { get; set; }
    }
    
    public class LocalSearchService
    {
        private LocalSearchSettings _localSearchSettings;

        public LocalSearchService(LocalSearchSettings localSearchSettings)
        {
            _localSearchSettings = localSearchSettings;
        }
        private static readonly HttpClient client = new HttpClient();

        public async Task<Dictionary<string, PointOfInterest>> getSearchData(Dictionary<string, PointOfInterest> pointsOfInterest)
        {
            var tasks = new List<Task<(string, JObject?)>>();

            // Getting data from extractedJsons to prepare local search request
            foreach (var kvp in pointsOfInterest)
            {
                // var poiId = kvp.Key;
                if (kvp.Value != null)
                {
                    string name = kvp.Value.Name ?? "";
                    double latitude = kvp.Value.Latitude ?? 0;
                    double longitude = kvp.Value.Longitude ?? 0;
                    
                    tasks.Add(SendRequestAsync(_localSearchSettings.Endpoint, _localSearchSettings.ApiKey, latitude, longitude, name));
                }
            }

            // result from local search
            var results = await Task.WhenAll(tasks);


            if (results.Length != pointsOfInterest.Count)
            {
                Console.WriteLine("Mismatch between LLM and LocalSearch data count.");
                // TODO handel properly 
            }

            foreach (var kvp in pointsOfInterest)
            {
                if (!int.TryParse(kvp.Key, out int index) || index < 0 || index >= results.Length)
                {
                    Console.WriteLine($"Invalid index or key: {kvp.Key}");
                    continue;
                }

                //Item2 is value of results KVP
                JObject? jsonObject = results[index - 1].Item2 as JObject;

                if (jsonObject != null)
                {
                    JArray? placesArray = jsonObject["data"]?["placeResponse"]?["places"] as JArray;

                    if (placesArray != null && placesArray.Count > 0)
                    {
                        JObject? place = placesArray[0] as JObject;

                        if (place != null)
                        {
                            JObject? geoPosition = place["location"]?["geoPosition"] as JObject;

                            if (geoPosition != null)
                            {
                                string name = place["name"]?.ToString() ?? ""; 
                                string id = place["id"]?.ToString() ?? "";
                                double latitude = double.Parse(geoPosition["latitude"]?.ToString() ?? "");
                                double longitude = double.Parse(geoPosition["longitude"]?.ToString() ?? "");

                                if (kvp.Value != null)
                                {
                                    // string valueAsString = kvp.Value.ToString();
                                    // dynamic parsedJson = JsonConvert.DeserializeObject(valueAsString);

                                    // Update latitude and longitude
                                    kvp.Value.Latitude = latitude;
                                    kvp.Value.Longitude = longitude;
                                    // Storing grounded lat, lng in extractedJsons (data from llm)
                                    // pointsOfInterest[kvp.Key] = parsedJson;
                                }
                            }
                            else
                            {
                                // TODO handel properly 
                                Console.WriteLine("geoPosition is not available.");
                            }
                        }
                        else
                        {
                            // TODO handel properly 
                            Console.WriteLine("place is not available.");
                        }
                    }
                    else
                    {
                        // TODO handel properly 
                        Console.WriteLine("place array is not available.");
                    }
                }
                else
                {   // TODO handel properly 
                    Console.WriteLine("No Data from Local Search.");
                }
            }

            return pointsOfInterest;
        }

        // HTTP call to local Search
        private static async Task<(string, JObject?)> SendRequestAsync(string apiUrl, string accessToken, double lat, double lon, string name)
        {
            var requestPayload = new
            {
                searchRequest = new
                {
                    searchItems = new[]
                    {
                        new { name = new { value = name } }
                    },
                    locationContext = new { geoPosition = $"{lat},{lon}" },
                    modifiers = new { units = "METRIC", limit = 1, radius = 10000 }
                }
            };

            string jsonPayload = JsonConvert.SerializeObject(requestPayload);

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Headers =
                {
                    { "Authorization", $"Bearer {accessToken}" },
                    { "Accept-Language", "en-US" }
                },
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject apiResponse = JObject.Parse(jsonResponse);
                return (name, apiResponse);
            }
            else
            {
                Console.WriteLine($"Error for location ({lat},{lon}): {response.StatusCode} - {response.ReasonPhrase}");
                return (name, null);
            }
        }
    }
}
