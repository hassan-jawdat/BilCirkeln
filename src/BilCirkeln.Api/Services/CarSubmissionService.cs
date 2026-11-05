using BilCirkeln.Api.Data;
using BilCirkeln.Api.DTOs;
using BilCirkeln.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BilCirkeln.Api.Services;

public interface ICarSubmissionService
{
    Task<ServiceResult<CarSubmissionDto>> CreateAsync(string customerId, CreateCarSubmissionDto dto);
    Task<List<CarSubmissionDto>> GetForCustomerAsync(string customerId);
    Task<List<CarSubmissionDto>> GetAllAsync(SubmissionStatus? status);
    Task<ServiceResult<CarSubmissionDto>> GetByIdAsync(int id, string userId, bool isStaff);
    Task<ServiceResult<CarSubmissionDto>> MakeOfferAsync(int id, string staffId, CreateOfferDto dto);
    Task<ServiceResult<CarSubmissionDto>> RespondToOfferAsync(int id, string customerId, bool accept);
    Task<ServiceResult<CarSubmissionDto>> RejectAsync(int id);
}

// All business rules for car submissions live here, not in the controller.
public class CarSubmissionService : ICarSubmissionService
{
    private readonly AppDbContext _db;

    public CarSubmissionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<CarSubmissionDto>> CreateAsync(string customerId, CreateCarSubmissionDto dto)
    {
        var regNumber = NormalizeRegNumber(dto.RegistrationNumber);

        // Rule: the same car cannot have two active submissions at the same time.
        var hasActive = await _db.CarSubmissions.AnyAsync(c =>
            c.RegistrationNumber == regNumber &&
            (c.Status == SubmissionStatus.Pending || c.Status == SubmissionStatus.OfferMade));

        if (hasActive)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.BadRequest,
                "Den här bilen har redan en aktiv förfrågan.");
        }

        var submission = new CarSubmission
        {
            RegistrationNumber = regNumber,
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            MileageKm = dto.MileageKm,
            FuelType = dto.FuelType,
            Gearbox = dto.Gearbox,
            Description = dto.Description?.Trim(),
            AskingPrice = dto.AskingPrice,
            KeyCount = dto.KeyCount,
            HasExtraTires = dto.HasExtraTires,
            HasServiceBook = dto.HasServiceBook,
            ServiceHistory = dto.ServiceHistory,
            InspectionStatus = dto.InspectionStatus,
            SellerType = dto.SellerType,
            IsImported = dto.IsImported,
            IsFinanced = dto.IsFinanced,
            InteriorColor = dto.InteriorColor,
            InteriorMaterial = dto.InteriorMaterial,
            CustomerId = customerId
        };

        _db.CarSubmissions.Add(submission);
        await _db.SaveChangesAsync();

        return await LoadAsDtoAsync(submission.Id);
    }

    public async Task<List<CarSubmissionDto>> GetForCustomerAsync(string customerId)
    {
        var list = await WithIncludes()
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return list.Select(ToDto).ToList();
    }

    public async Task<List<CarSubmissionDto>> GetAllAsync(SubmissionStatus? status)
    {
        var query = WithIncludes();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return list.Select(ToDto).ToList();
    }

    public async Task<ServiceResult<CarSubmissionDto>> GetByIdAsync(int id, string userId, bool isStaff)
    {
        var submission = await WithIncludes().FirstOrDefaultAsync(c => c.Id == id);
        if (submission == null)
        {
            return NotFound();
        }

        // A customer may only see their own submissions. Staff may see all.
        if (!isStaff && submission.CustomerId != userId)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.Forbidden,
                "Du har inte behörighet till den här förfrågan.");
        }

        return ServiceResult<CarSubmissionDto>.Ok(ToDto(submission));
    }

    public async Task<ServiceResult<CarSubmissionDto>> MakeOfferAsync(int id, string staffId, CreateOfferDto dto)
    {
        var submission = await _db.CarSubmissions.FindAsync(id);
        if (submission == null)
        {
            return NotFound();
        }

        // Rule: an offer can only be made on a new submission, or after the customer declined.
        if (submission.Status != SubmissionStatus.Pending && submission.Status != SubmissionStatus.Declined)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.BadRequest,
                $"Kan inte lämna bud när status är {submission.Status}.");
        }

        _db.Offers.Add(new Offer
        {
            CarSubmissionId = submission.Id,
            Amount = dto.Amount,
            Message = dto.Message?.Trim(),
            StaffId = staffId
        });
        submission.Status = SubmissionStatus.OfferMade;

        await _db.SaveChangesAsync();
        return await LoadAsDtoAsync(id);
    }

    public async Task<ServiceResult<CarSubmissionDto>> RespondToOfferAsync(int id, string customerId, bool accept)
    {
        var submission = await _db.CarSubmissions
            .Include(c => c.Offers)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (submission == null)
        {
            return NotFound();
        }

        if (submission.CustomerId != customerId)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.Forbidden,
                "Du kan bara svara på bud för dina egna bilar.");
        }

        var openOffer = submission.Offers.FirstOrDefault(o => o.Status == OfferStatus.Open);
        if (submission.Status != SubmissionStatus.OfferMade || openOffer == null)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.BadRequest,
                "Det finns inget bud att svara på.");
        }

        openOffer.Status = accept ? OfferStatus.Accepted : OfferStatus.Declined;
        submission.Status = accept ? SubmissionStatus.Accepted : SubmissionStatus.Declined;

        await _db.SaveChangesAsync();
        return await LoadAsDtoAsync(id);
    }

    public async Task<ServiceResult<CarSubmissionDto>> RejectAsync(int id)
    {
        var submission = await _db.CarSubmissions.FindAsync(id);
        if (submission == null)
        {
            return NotFound();
        }

        if (submission.Status != SubmissionStatus.Pending && submission.Status != SubmissionStatus.Declined)
        {
            return ServiceResult<CarSubmissionDto>.Fail(ServiceError.BadRequest,
                $"Kan inte avböja när status är {submission.Status}.");
        }

        submission.Status = SubmissionStatus.Rejected;
        await _db.SaveChangesAsync();
        return await LoadAsDtoAsync(id);
    }

    // "abc 123" -> "ABC123", so the same car is always stored the same way.
    public static string NormalizeRegNumber(string regNumber) =>
        regNumber.Replace(" ", "").Trim().ToUpperInvariant();

    private IQueryable<CarSubmission> WithIncludes() =>
        _db.CarSubmissions
            .Include(c => c.Customer)
            .Include(c => c.Offers);

    private async Task<ServiceResult<CarSubmissionDto>> LoadAsDtoAsync(int id)
    {
        var submission = await WithIncludes().FirstAsync(c => c.Id == id);
        return ServiceResult<CarSubmissionDto>.Ok(ToDto(submission));
    }

    private static ServiceResult<CarSubmissionDto> NotFound() =>
        ServiceResult<CarSubmissionDto>.Fail(ServiceError.NotFound, "Förfrågan hittades inte.");

    // Converts the database entity to the DTO we send to the frontend.
    private static CarSubmissionDto ToDto(CarSubmission c) => new()
    {
        Id = c.Id,
        RegistrationNumber = c.RegistrationNumber,
        Make = c.Make,
        Model = c.Model,
        Year = c.Year,
        MileageKm = c.MileageKm,
        FuelType = c.FuelType,
        Gearbox = c.Gearbox,
        Description = c.Description,
        AskingPrice = c.AskingPrice,
        KeyCount = c.KeyCount,
        HasExtraTires = c.HasExtraTires,
        HasServiceBook = c.HasServiceBook,
        ServiceHistory = c.ServiceHistory,
        InspectionStatus = c.InspectionStatus,
        SellerType = c.SellerType,
        IsImported = c.IsImported,
        IsFinanced = c.IsFinanced,
        InteriorColor = c.InteriorColor,
        InteriorMaterial = c.InteriorMaterial,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        CustomerName = c.Customer?.FullName ?? string.Empty,
        CustomerEmail = c.Customer?.Email ?? string.Empty,
        Offers = c.Offers
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OfferDto(o.Id, o.Amount, o.Message, o.Status, o.CreatedAt))
            .ToList()
    };
}
