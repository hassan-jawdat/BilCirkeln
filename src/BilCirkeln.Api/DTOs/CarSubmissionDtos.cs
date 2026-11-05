using System.ComponentModel.DataAnnotations;
using BilCirkeln.Api.Models;

namespace BilCirkeln.Api.DTOs;

// Data the customer sends when submitting a car.
public class CreateCarSubmissionDto
{
    // Swedish format: ABC123 or ABC12D (space optional).
    [Required, RegularExpression(@"^[A-Za-z]{3}\s?\d{2}[A-Za-z0-9]$",
        ErrorMessage = "Registreringsnumret måste se ut som ABC123 eller ABC12D.")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1950, 2100)]
    public int Year { get; set; }

    [Range(0, 2_000_000)]
    public int MileageKm { get; set; }

    [EnumDataType(typeof(FuelType))]
    public FuelType FuelType { get; set; }

    [EnumDataType(typeof(GearboxType))]
    public GearboxType Gearbox { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 10_000_000)]
    public decimal AskingPrice { get; set; }

    // Extra details. Nullable + [Required] means: the API answers 400 if a field is missing,
    // instead of silently using the default value (false / the first enum value).
    [Required] public KeyCount? KeyCount { get; set; }
    [Required] public bool? HasExtraTires { get; set; }
    [Required] public bool? HasServiceBook { get; set; }
    [Required] public ServiceHistory? ServiceHistory { get; set; }
    [Required] public InspectionStatus? InspectionStatus { get; set; }
    [Required] public SellerType? SellerType { get; set; }
    [Required] public bool? IsImported { get; set; }
    [Required] public bool? IsFinanced { get; set; }
    [Required] public InteriorColor? InteriorColor { get; set; }
    [Required] public InteriorMaterial? InteriorMaterial { get; set; }
}

// Data staff sends when making an offer.
public class CreateOfferDto
{
    [Range(1, 10_000_000)]
    public decimal Amount { get; set; }

    [StringLength(1000)]
    public string? Message { get; set; }
}

// The customer's answer to an offer.
public class RespondToOfferDto
{
    public bool Accept { get; set; }
}

public record OfferDto(
    int Id,
    decimal Amount,
    string? Message,
    OfferStatus Status,
    DateTime CreatedAt);

// What the API returns for a submission. We never return the entity itself,
// so we control exactly which fields are exposed (e.g. no password hash).
public class CarSubmissionDto
{
    public int Id { get; init; }
    public string RegistrationNumber { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int MileageKm { get; init; }
    public FuelType FuelType { get; init; }
    public GearboxType Gearbox { get; init; }
    public string? Description { get; init; }
    public decimal AskingPrice { get; init; }

    public KeyCount? KeyCount { get; init; }
    public bool? HasExtraTires { get; init; }
    public bool? HasServiceBook { get; init; }
    public ServiceHistory? ServiceHistory { get; init; }
    public InspectionStatus? InspectionStatus { get; init; }
    public SellerType? SellerType { get; init; }
    public bool? IsImported { get; init; }
    public bool? IsFinanced { get; init; }
    public InteriorColor? InteriorColor { get; init; }
    public InteriorMaterial? InteriorMaterial { get; init; }

    public SubmissionStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public List<OfferDto> Offers { get; init; } = new();
}
