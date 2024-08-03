// See https://aka.ms/new-console-template for more information
using Util;
using static Util.BagOfWordsHelper;

Console.WriteLine("This is an example of how you might use the Bag of Words Pattern to build a vocabulary that can be used for Intent Recognition");

// Sample list of PluginData
var plugins = new List<PluginData>
        {
            new PluginData { Description = "A powerful tool for data analysis", PluginName = "DataAnalyzer", FunctionName = "Analyze" },
            new PluginData { Description = "An efficient plugin for image processing", PluginName = "ImageProcessor", FunctionName = "Process" },
            new PluginData { Description = "Help users with File Management, Need help with files", PluginName = "FileManager", FunctionName = "Manage" },
            new PluginData { Description = "Get the current time", PluginName = "time_plugin", FunctionName = "get_time" },
            new PluginData { Description = "Help users plan trips", PluginName = "trip_planner", FunctionName = "plan_trip" },
            new PluginData { Description = "Help users modify trips", PluginName = "trip_planner", FunctionName = "modify_trip" },
        };

var testprompt = "Can you help me plan a trip?";
var bestMatch = BagOfWordsHelper.TestBagOfWords(plugins, testprompt);
Console.WriteLine($"User Prompt: {testprompt}");
Console.WriteLine($"Best Match: Plugin:{plugins[bestMatch.Index].PluginName} FunctionName:{plugins[bestMatch.Index].FunctionName}");

Console.WriteLine("Lets run another test!");
testprompt = "What is the time?";
bestMatch = BagOfWordsHelper.TestBagOfWords(plugins, testprompt);
Console.WriteLine($"User Prompt: {testprompt}");
Console.WriteLine($"Best Match: Plugin:{plugins[bestMatch.Index].PluginName} FunctionName:{plugins[bestMatch.Index].FunctionName}");

Console.WriteLine("Lets run another test!");
testprompt = "Can you help me with FileManagement?";
bestMatch = BagOfWordsHelper.TestBagOfWords(plugins, testprompt);
Console.WriteLine($"User Prompt: {testprompt}");
Console.WriteLine($"Best Match: Plugin:{plugins[bestMatch.Index].PluginName} FunctionName:{plugins[bestMatch.Index].FunctionName}");

