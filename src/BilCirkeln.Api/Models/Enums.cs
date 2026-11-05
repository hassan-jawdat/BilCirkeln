namespace BilCirkeln.Api.Models;

public enum FuelType
{
    Petrol,
    Diesel,
    Hybrid,
    Electric
}

public enum GearboxType
{
    Manual,
    Automatic
}

public enum KeyCount
{
    One,
    TwoOrMore
}

// How many of the services have been done and documented.
public enum ServiceHistory
{
    All,
    Some,
    None
}

// Has the car been inspected (besiktigad) in the last 6 months?
public enum InspectionStatus
{
    Recent,
    NotRecent,
    Expired
}

public enum SellerType
{
    Private,
    CompanyWithVat
}

public enum InteriorColor
{
    Black,
    Grey,
    Beige,
    Blue,
    White,
    Other
}

public enum InteriorMaterial
{
    Fabric,
    Leather,
    PartLeather,
    Alcantara,
    Plastic,
    Other
}

// Flow: Pending -> OfferMade -> Accepted
//                            -> Declined -> (new offer) -> OfferMade ...
// Staff can reject a Pending or Declined submission.
public enum SubmissionStatus
{
    Pending,
    OfferMade,
    Accepted,
    Declined,
    Rejected
}

public enum OfferStatus
{
    Open,
    Accepted,
    Declined
}

// The role names used in the whole app.
public static class Roles
{
    public const string Customer = "Kund";
    public const string Staff = "Personal";
}
