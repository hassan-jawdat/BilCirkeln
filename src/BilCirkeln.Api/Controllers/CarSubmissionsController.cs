using BilCirkeln.Api.DTOs;
using BilCirkeln.Api.Models;
using BilCirkeln.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BilCirkeln.Api.Controllers;

[Route("api/submissions")]
[Authorize] // Every endpoint here requires a valid JWT token.
public class CarSubmissionsController : ApiControllerBase
{
    private readonly ICarSubmissionService _service;

    public CarSubmissionsController(ICarSubmissionService service)
    {
        _service = service;
    }

    // POST api/submissions - customer submits a car
    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create(CreateCarSubmissionDto dto) =>
        ToActionResult(await _service.CreateAsync(CurrentUserId, dto));

    // GET api/submissions/mine - customer's own submissions
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetMine() =>
        Ok(await _service.GetForCustomerAsync(CurrentUserId));

    // GET api/submissions?status=Pending - staff sees all (optional filter)
    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> GetAll([FromQuery] SubmissionStatus? status) =>
        Ok(await _service.GetAllAsync(status));

    // GET api/submissions/5 - customer (own) or staff (all)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        ToActionResult(await _service.GetByIdAsync(id, CurrentUserId, User.IsInRole(Roles.Staff)));

    // POST api/submissions/5/offers - staff makes an offer
    [HttpPost("{id:int}/offers")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> MakeOffer(int id, CreateOfferDto dto) =>
        ToActionResult(await _service.MakeOfferAsync(id, CurrentUserId, dto));

    // POST api/submissions/5/respond - customer accepts or declines the offer
    [HttpPost("{id:int}/respond")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Respond(int id, RespondToOfferDto dto) =>
        ToActionResult(await _service.RespondToOfferAsync(id, CurrentUserId, dto.Accept));

    // POST api/submissions/5/reject - staff is not interested in the car
    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Reject(int id) =>
        ToActionResult(await _service.RejectAsync(id));
}
