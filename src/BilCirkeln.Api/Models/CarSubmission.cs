namespace BilCirkeln.Api.Models;

// A car that a customer wants to sell to us.
public class CarSubmission
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int MileageKm { get; set; }
    public FuelType FuelType { get; set; }
    public GearboxType Gearbox { get; set; }
    public string? Description { get; set; }
    public decimal AskingPrice { get; set; }

    // Extra details about the car. Nullable because submissions made before
    // these questions were added do not have the answers.
    public KeyCount? KeyCount { get; set; }
    public bool? HasExtraTires { get; set; }
    public bool? HasServiceBook { get; set; }
    public ServiceHistory? ServiceHistory { get; set; }
    public InspectionStatus? InspectionStatus { get; set; }
    public SellerType? SellerType { get; set; }
    public bool? IsImported { get; set; }
    public bool? IsFinanced { get; set; }
    public InteriorColor? InteriorColor { get; set; }
    public InteriorMaterial? InteriorMaterial { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to the customer who owns the submission.
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser? Customer { get; set; }

    // A submission can get several offers over time (e.g. a new offer after a declined one).
    public List<Offer> Offers { get; set; } = new();
}
