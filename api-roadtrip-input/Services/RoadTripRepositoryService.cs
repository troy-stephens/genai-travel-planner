
using api_roadtrip_input.Interfaces;
using api_roadtrip_input.Models.RoadTripDoc;
using api_roadtrip_input.Models.VendorRoadTripDoc;
using Microsoft.Azure.Cosmos;

namespace api_roadtrip_input.Services;
public class RoadTripRepositoryService : IRoadTripRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _userRoadTripContainer;
    private readonly Container _vendorRoadTripContainer;

    public RoadTripRepositoryService(CosmosClient cosmosClient, string databaseName, string userRoadTripContainerName, string vendorRoadTripContainerName)
    {
        _cosmosClient = cosmosClient;
        _userRoadTripContainer = _cosmosClient.GetContainer(databaseName, userRoadTripContainerName);
        _vendorRoadTripContainer = _cosmosClient.GetContainer(databaseName, vendorRoadTripContainerName);
    }

    public async Task<RoadTripDocument?> GetRoadTripAsync(string id)
    {
        try
        {
            ItemResponse<RoadTripDocument> response = await _userRoadTripContainer.ReadItemAsync<RoadTripDocument>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<VendorRoadTripDocument?> GetVendorRoadTripAsync(string id)
    {
        try
        {
            ItemResponse<VendorRoadTripDocument> response = await _vendorRoadTripContainer.ReadItemAsync<VendorRoadTripDocument>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<RoadTripDocument>> GetRoadTripsByUserIdAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
            .WithParameter("@userId", userId);

        var iterator = _userRoadTripContainer.GetItemQueryIterator<RoadTripDocument>(query);
        List<RoadTripDocument> results = new List<RoadTripDocument>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task<IEnumerable<VendorRoadTripDocument>> GetVendorRoadTripsByVendorIdAsync(string vendorId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.VendorId = @vendorId")
            .WithParameter("@vendorId", vendorId);

        var iterator = _vendorRoadTripContainer.GetItemQueryIterator<VendorRoadTripDocument>(query);
        List<VendorRoadTripDocument> results = new List<VendorRoadTripDocument>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task AddRoadTripAsync(RoadTripDocument roadTripDocument)
    {
        await _userRoadTripContainer.CreateItemAsync(roadTripDocument, new PartitionKey(roadTripDocument.Id));
    }

    public async Task AddVendorRoadTripAsync(VendorRoadTripDocument vendorRoadTripDocument)
    {
        await _vendorRoadTripContainer.CreateItemAsync(vendorRoadTripDocument, new PartitionKey(vendorRoadTripDocument.Id));
    }

    public async Task UpdateRoadTripAsync(RoadTripDocument roadTripDocument)
    {
        await _userRoadTripContainer.UpsertItemAsync(roadTripDocument, new PartitionKey(roadTripDocument.Id));
    }

    public async Task UpdateVendorRoadTripAsync(VendorRoadTripDocument vendorRoadTripDocument)
    {
        await _vendorRoadTripContainer.UpsertItemAsync(vendorRoadTripDocument, new PartitionKey(vendorRoadTripDocument.Id));
    }

    public async Task DeleteRoadTripAsync(string id)
    {
        await _userRoadTripContainer.DeleteItemAsync<RoadTripDocument>(id, new PartitionKey(id));
    }

    public async Task DeleteVendorRoadTripAsync(string id)
    {
        await _vendorRoadTripContainer.DeleteItemAsync<VendorRoadTripDocument>(id, new PartitionKey(id));
    }
}

