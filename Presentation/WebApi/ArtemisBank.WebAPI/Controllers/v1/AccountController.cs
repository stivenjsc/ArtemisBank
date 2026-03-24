using ArtemisBank.Core.Application.DTOs.Account;
using ArtemisBank.Core.Application.Interfaces.IServices;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
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
        }
    }
}
