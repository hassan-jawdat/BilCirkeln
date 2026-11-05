namespace BilCirkeln.Api.Models;

// A price offer that staff gives on a car submission.
public class Offer
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string? Message { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CarSubmissionId { get; set; }
    public CarSubmission? CarSubmission { get; set; }

    // The staff member who made the offer.
    public string StaffId { get; set; } = string.Empty;
    public ApplicationUser? Staff { get; set; }
}
