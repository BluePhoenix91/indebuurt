namespace Pipeline.Core.Enums;

/// <summary>
/// Fields from neighborhood statistics that can be evaluated in label rules.
/// Maps to computed values from GIS data.
/// </summary>
public enum ConditionField
{
    // Amenities
    ParkCount,
    DogParkCount,
    VetCount,
    PetStoreCount,
    SupermarketCount,

    // Transport
    TransitStopCount,
    TrainStationDistance,

    // Demographics
    PopulationDensity,
    MedianHousePrice,
    OwnershipRate
}
