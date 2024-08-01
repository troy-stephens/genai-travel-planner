using System.Collections.Generic;
using System.Threading.Tasks;
using api_roadtrip_input.Models.RoadTripDoc;
using api_roadtrip_input.Models.VendorRoadTripDoc;

namespace api_roadtrip_input.Interfaces;
public interface IRoadTripRepository
{
    Task<RoadTripDocument?> GetRoadTripAsync(string id);
    Task<IEnumerable<RoadTripDocument>> GetRoadTripsByUserIdAsync(string userId);
    Task AddRoadTripAsync(RoadTripDocument roadTripDocument);
    Task UpdateRoadTripAsync(RoadTripDocument roadTripDocument);
    Task DeleteRoadTripAsync(string id);

    Task<VendorRoadTripDocument?> GetVendorRoadTripAsync(string id);
    Task<IEnumerable<VendorRoadTripDocument>> GetVendorRoadTripsByVendorIdAsync(string userId);
    Task AddVendorRoadTripAsync(VendorRoadTripDocument roadTripDocument);
    Task UpdateVendorRoadTripAsync(VendorRoadTripDocument roadTripDocument);
    Task DeleteVendorRoadTripAsync(string id);
}
