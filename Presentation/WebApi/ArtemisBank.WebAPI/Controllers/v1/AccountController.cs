using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Infrastructure.Identity.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AccountController(IUserService userService, UserManager<ApplicationUser> userManager,IJwtService jwtService) : BaseApiController
    {
        private readonly IUserService _userService = userService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtService _jwtService = jwtService;

        [HttpGet("activate")]
        public async Task<IActionResult> Activate([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "The activation link is invalid." });
            }

            var result = await _userService.ActivateAccountAsync(token);
            if (!result)
            {
                return BadRequest(new { message = "Invalid activation link." });
            }
            return Ok(new { message = "Your account has been activated successfully." });
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return StatusCode(403, new { message = "You do not have permission to access this resource." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Usuario y contraseña son requeridos." });

            var result = await _userService.AuthenticateAsync(request.UserName, request.Password);

            if (!result.Success)
                return Unauthorized(new { message = result.Error });

            var token = await _jwtService.GenerateTokenAsync(
                result.UserId,
                result.UserName,
                result.Email,
                new[] { result.Role.ToString() },
                result.CommerceId);

            return Ok(new { Jwt = token });
        }
    }
    #region public recorded

    public record LoginRequest(string UserName, string Password);
    public record ConfirmAccountRequest(string Token);
    public record GetResetTokenRequest(string UserName);
    public record ResetPasswordRequest(
        string UserId,
        string Token,
        string Password,
        string ConfirmPassword);

    #endregion
}