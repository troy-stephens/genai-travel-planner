using System;
using System.Collections.Generic;

namespace api_roadtrip_test.Models.RoadTripDoc;
public class OpeningHours
{
    public string? Monday { get; set; }
    public string? Tuesday { get; set; }
    public string? Wednesday { get; set; }
    public string? Thursday { get; set; }
    public string? Friday { get; set; }
    public string? Saturday { get; set; }
    public string? Sunday { get; set; }
}

public class Costs
{
    public string? Adults { get; set; }
    public string? Children { get; set; }
}

public class PointOfInterest
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Address? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Description { get; set; }
    public string? Category { get; set; }
    public OpeningHours? OpeningHours { get; set; }
    public Costs? Costs { get; set; }
    public string? Website { get; set; }
    public string? Contact { get; set; }
}

public class Address
{
    public string? HouseNumber { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? StateCode { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? AddressLabel { get; set; }
    public string? PostalCode { get; set; }
    public string? Locality { get; set; }
}

public class RoadTrip
{
    public string? Name { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public List<PointOfInterest>? PointsOfInterest { get; set; }
}

public class RoadTripDocument
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string? DocType { get; set; }
    public string? RoadTripId { get; set; }
    public RoadTrip? RoadTrip { get; set; }
}

