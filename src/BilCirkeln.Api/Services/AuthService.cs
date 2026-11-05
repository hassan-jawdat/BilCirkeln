using BilCirkeln.Api.DTOs;
using BilCirkeln.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace BilCirkeln.Api.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    // UserManager is Identity's class for creating users, checking passwords, roles etc.
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        // Identity hashes the password - we never store it in plain text.
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return ServiceResult<AuthResponseDto>.Fail(ServiceError.BadRequest, errors);
        }

        // Everyone who registers themselves becomes a customer.
        // Staff accounts are created by the seed, so nobody can make themselves staff.
        await _userManager.AddToRoleAsync(user, Roles.Customer);

        return ServiceResult<AuthResponseDto>.Ok(CreateResponse(user, Roles.Customer));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Same message for "wrong email" and "wrong password",
        // so an attacker cannot find out which emails exist.
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return ServiceResult<AuthResponseDto>.Fail(ServiceError.BadRequest, "Fel e-post eller lösenord.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Contains(Roles.Staff) ? Roles.Staff : Roles.Customer;

        return ServiceResult<AuthResponseDto>.Ok(CreateResponse(user, role));
    }

    private AuthResponseDto CreateResponse(ApplicationUser user, string role) =>
        new(_tokenService.CreateToken(user, role), user.Email!, user.FullName, role);
}
