<<<<<<< HEAD
﻿using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Infrastructure.Identity.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
=======
﻿using ArtemisBank.Core.Application.DTOs.Account;
using ArtemisBank.Core.Application.Interfaces.IServices;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
>>>>>>> 2bc3dcfcb3ebd68c72cf1e92659b6ba1c34eb1d5

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
<<<<<<< HEAD
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
=======
    [Route("account")]
    public class AccountController : BaseApiController
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.AuthenticateAsync(dto.UserName, dto.Password);

            if (result == null || !result.Success)
            {
                return Unauthorized(new { message = result?.Error ?? "Invalid username or password." });
            }

            return Ok(new { Jwt = result.JwtToken });
        }

        /// <summary>
        /// Confirm user account with token
        /// </summary>
        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ActivateAccountAsync(dto.Token);

            if (!result)
            {
                return BadRequest(new { message = "Invalid or expired token." });
            }

            return NoContent();
        }

        /// <summary>
        /// Request password reset token
        /// </summary>
        [HttpPost("get-reset-token")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetResetToken([FromBody] GetResetTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.GeneratePasswordResetTokenAsync(dto.UserName);

            if (!result)
            {
                return BadRequest(new { message = "User not found or invalid." });
            }

            return NoContent();
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }

            var result = await _userService.ResetPasswordAsync(dto.UserId, dto.Token, dto.Password);

            if (!result)
            {
                return BadRequest(new { message = "Invalid token or user." });
            }

            return NoContent();
>>>>>>> 2bc3dcfcb3ebd68c72cf1e92659b6ba1c34eb1d5
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