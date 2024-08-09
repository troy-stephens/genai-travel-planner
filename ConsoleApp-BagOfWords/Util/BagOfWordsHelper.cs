using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Util.BagOfWordsHelper;

namespace Util
{
    internal static class BagOfWordsHelper
    {
        public static (int Index, double Similarity) TestBagOfWords(List<PluginData> plugins, string queryPrompt)
        {
            try
            {
                // Create a new ML.NET context  
                var context = new MLContext();

                // Load the data into an IDataView  
                var trainingData = context.Data.LoadFromEnumerable(plugins);

                // Create a pipeline to transform the text into Bag of Words  
                var pipeline = context.Transforms.Text.FeaturizeText("Features", nameof(PluginData.Description));

                // Fit the pipeline to the data  
                var model = pipeline.Fit(trainingData);

                // Transform the data  
                var transformedData = model.Transform(trainingData);

                // Get the feature vectors from the transformed data  
                var featureVectors = context.Data.CreateEnumerable<OutputData>(transformedData, reuseRowObject: false).ToArray();

                // Convert the query description into a feature vector  
                var queryVector = GetFeatureVector(context, model, queryPrompt);

                // Display the cosine similarities  
                for (int i = 0; i < plugins.Count; i++)
                {
                    var similarity = CosineSimilarity(featureVectors, queryVector, i);
                    Console.WriteLine($"Cosine similarity between query and {plugins[i].PluginName}: {similarity}");
                }

                // Find the best match using tuples  
                var bestMatch = featureVectors
                    .Select((vec, index) => (Index: index, Similarity: CosineSimilarity(featureVectors, queryVector, index)))
                    .OrderByDescending(x => x.Similarity)
                    .First();

                Console.WriteLine($"Best match is at index {bestMatch.Index} with similarity: {bestMatch.Similarity}");
                return bestMatch; // Return the index and similarity as a tuple  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return (-1, 0); // Return a default value indicating an error  
            }
        }

        // Method to convert a query string into a feature vector  
        private static float[] GetFeatureVector(MLContext context, ITransformer model, string query)
        {
            try
            {
                var queryData = new List<PluginData> { new PluginData { Description = query } };
                var queryDataView = context.Data.LoadFromEnumerable(queryData);
                var transformedQueryData = model.Transform(queryDataView);
                var outputData = context.Data.CreateEnumerable<OutputData>(transformedQueryData, reuseRowObject: false).ToArray();
                return outputData[0].Features ?? Array.Empty<float>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating the feature vector: {ex.Message}");
                return Array.Empty<float>(); // Return an empty array indicating an error  
            }
        }

        static double CosineSimilarity(OutputData[] featureVectors, float[] queryVector, int targetIndex)
        {
            try
            {
                var targetVector = featureVectors[targetIndex].Features;
                #pragma warning disable CS8604 // Possible null reference argument.
                double dotProduct = queryVector.Zip(targetVector, (a, b) => a * b).Sum();
                double magnitudeQuery = Math.Sqrt(queryVector.Sum(x => x * x));
                double magnitudeTarget = Math.Sqrt(targetVector.Sum(x => x * x));

                return dotProduct / (magnitudeQuery * magnitudeTarget);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating the Cosine Similarity: {ex.Message}");
                return 0; // Return a default value indicating an error  
            }
        }

        // Define the PluginData class
        public class PluginData
        {
            public string? Description { get; set; }
            public string? PluginName { get; set; }
            public string? FunctionName { get; set; }
        }

        // Define a class to hold the output data
        public class OutputData
        {
            public float[]? Features { get; set; }
        }

    }
}
