using BilCirkeln.Api.Data;
using BilCirkeln.Api.DTOs;
using BilCirkeln.Api.Models;
using BilCirkeln.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BilCirkeln.Tests;

// Tests the business rules in CarSubmissionService.
// We use an in-memory database so the tests are fast and need no SQL Server.
public class CarSubmissionServiceTests
{
    private const string CustomerId = "customer-1";
    private const string OtherCustomerId = "customer-2";
    private const string StaffId = "staff-1";

    private readonly AppDbContext _db;
    private readonly CarSubmissionService _service;

    // xUnit runs the constructor before every test, so each test gets a fresh database.
    public CarSubmissionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _db.Users.AddRange(
            new ApplicationUser { Id = CustomerId, Email = "kund@test.se", FullName = "Kund Ett" },
            new ApplicationUser { Id = OtherCustomerId, Email = "annan@test.se", FullName = "Kund Två" },
            new ApplicationUser { Id = StaffId, Email = "personal@test.se", FullName = "Personal" });
        _db.SaveChanges();

        _service = new CarSubmissionService(_db);
    }

    private static CreateCarSubmissionDto NewCar(string regNumber = "ABC123") => new()
    {
        RegistrationNumber = regNumber,
        Make = "Volvo",
        Model = "V70",
        Year = 2015,
        MileageKm = 150000,
        FuelType = FuelType.Diesel,
        Gearbox = GearboxType.Automatic,
        AskingPrice = 95000,
        KeyCount = KeyCount.TwoOrMore,
        HasExtraTires = true,
        HasServiceBook = true,
        ServiceHistory = ServiceHistory.All,
        InspectionStatus = InspectionStatus.Recent,
        SellerType = SellerType.Private,
        IsImported = false,
        IsFinanced = false,
        InteriorColor = InteriorColor.Black,
        InteriorMaterial = InteriorMaterial.Leather
    };

    private async Task<int> CreateCarAsync()
    {
        var result = await _service.CreateAsync(CustomerId, NewCar());
        return result.Data!.Id;
    }

    [Fact]
    public async Task Create_NewSubmission_IsPendingAndRegNumberIsNormalized()
    {
        var result = await _service.CreateAsync(CustomerId, NewCar("abc 123"));

        Assert.True(result.Success);
        Assert.Equal(SubmissionStatus.Pending, result.Data!.Status);
        Assert.Equal("ABC123", result.Data.RegistrationNumber);
        Assert.Equal("Kund Ett", result.Data.CustomerName);
    }

    [Fact]
    public async Task Create_SavesExtraCarDetails()
    {
        var result = await _service.CreateAsync(CustomerId, NewCar());

        var saved = await _service.GetByIdAsync(result.Data!.Id, CustomerId, isStaff: false);

        Assert.Equal(KeyCount.TwoOrMore, saved.Data!.KeyCount);
        Assert.True(saved.Data.HasExtraTires);
        Assert.Equal(ServiceHistory.All, saved.Data.ServiceHistory);
        Assert.Equal(InspectionStatus.Recent, saved.Data.InspectionStatus);
        Assert.False(saved.Data.IsFinanced);
        Assert.Equal(InteriorMaterial.Leather, saved.Data.InteriorMaterial);
    }

    [Fact]
    public async Task Create_SameCarAlreadyActive_ReturnsBadRequest()
    {
        await _service.CreateAsync(CustomerId, NewCar("ABC123"));

        var result = await _service.CreateAsync(OtherCustomerId, NewCar("abc 123"));

        Assert.False(result.Success);
        Assert.Equal(ServiceError.BadRequest, result.Error);
    }

    [Fact]
    public async Task GetForCustomer_ReturnsOnlyOwnSubmissions()
    {
        await _service.CreateAsync(CustomerId, NewCar("ABC123"));
        await _service.CreateAsync(OtherCustomerId, NewCar("XYZ789"));

        var mine = await _service.GetForCustomerAsync(CustomerId);

        Assert.Single(mine);
        Assert.Equal("ABC123", mine[0].RegistrationNumber);
    }

    [Fact]
    public async Task GetById_OtherCustomersSubmission_ReturnsForbidden()
    {
        var id = await CreateCarAsync();

        var result = await _service.GetByIdAsync(id, OtherCustomerId, isStaff: false);

        Assert.Equal(ServiceError.Forbidden, result.Error);
    }

    [Fact]
    public async Task GetById_StaffCanSeeAnySubmission()
    {
        var id = await CreateCarAsync();

        var result = await _service.GetByIdAsync(id, StaffId, isStaff: true);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsOnlyMatching()
    {
        var id = await CreateCarAsync();
        await _service.CreateAsync(OtherCustomerId, NewCar("XYZ789"));
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });

        var pending = await _service.GetAllAsync(SubmissionStatus.Pending);

        Assert.Single(pending);
        Assert.Equal("XYZ789", pending[0].RegistrationNumber);
    }

    [Fact]
    public async Task MakeOffer_OnPending_ChangesStatusToOfferMade()
    {
        var id = await CreateCarAsync();

        var result = await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });

        Assert.True(result.Success);
        Assert.Equal(SubmissionStatus.OfferMade, result.Data!.Status);
        Assert.Single(result.Data.Offers);
        Assert.Equal(OfferStatus.Open, result.Data.Offers[0].Status);
    }

    [Fact]
    public async Task MakeOffer_WhenOfferAlreadyOpen_ReturnsBadRequest()
    {
        var id = await CreateCarAsync();
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });

        var result = await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 85000 });

        Assert.Equal(ServiceError.BadRequest, result.Error);
    }

    [Fact]
    public async Task MakeOffer_UnknownSubmission_ReturnsNotFound()
    {
        var result = await _service.MakeOfferAsync(999, StaffId, new CreateOfferDto { Amount = 80000 });

        Assert.Equal(ServiceError.NotFound, result.Error);
    }

    [Fact]
    public async Task Respond_Accept_MarksSubmissionAndOfferAccepted()
    {
        var id = await CreateCarAsync();
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });

        var result = await _service.RespondToOfferAsync(id, CustomerId, accept: true);

        Assert.Equal(SubmissionStatus.Accepted, result.Data!.Status);
        Assert.Equal(OfferStatus.Accepted, result.Data.Offers[0].Status);
    }

    [Fact]
    public async Task Respond_Decline_ThenStaffCanMakeNewOffer()
    {
        var id = await CreateCarAsync();
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });
        await _service.RespondToOfferAsync(id, CustomerId, accept: false);

        var result = await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 88000 });

        Assert.Equal(SubmissionStatus.OfferMade, result.Data!.Status);
        Assert.Equal(2, result.Data.Offers.Count);
    }

    [Fact]
    public async Task Respond_WithoutOffer_ReturnsBadRequest()
    {
        var id = await CreateCarAsync();

        var result = await _service.RespondToOfferAsync(id, CustomerId, accept: true);

        Assert.Equal(ServiceError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Respond_ByOtherCustomer_ReturnsForbidden()
    {
        var id = await CreateCarAsync();
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });

        var result = await _service.RespondToOfferAsync(id, OtherCustomerId, accept: true);

        Assert.Equal(ServiceError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Reject_Pending_ChangesStatusToRejected()
    {
        var id = await CreateCarAsync();

        var result = await _service.RejectAsync(id);

        Assert.Equal(SubmissionStatus.Rejected, result.Data!.Status);
    }

    [Fact]
    public async Task Reject_AcceptedSubmission_ReturnsBadRequest()
    {
        var id = await CreateCarAsync();
        await _service.MakeOfferAsync(id, StaffId, new CreateOfferDto { Amount = 80000 });
        await _service.RespondToOfferAsync(id, CustomerId, accept: true);

        var result = await _service.RejectAsync(id);

        Assert.Equal(ServiceError.BadRequest, result.Error);
    }
}
