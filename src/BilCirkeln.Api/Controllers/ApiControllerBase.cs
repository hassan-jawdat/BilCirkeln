using System.Security.Claims;
using BilCirkeln.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilCirkeln.Api.Controllers;

// Shared helpers for all our controllers.
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // The user id is read from the JWT token, never from the request body,
    // so a user cannot pretend to be someone else.
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Turns a ServiceResult into the right HTTP response.
    protected IActionResult ToActionResult<T>(ServiceResult<T> result) => result.Error switch
    {
        ServiceError.None => Ok(result.Data),
        ServiceError.NotFound => NotFound(new { message = result.Message }),
        ServiceError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
        _ => BadRequest(new { message = result.Message })
    };
}
