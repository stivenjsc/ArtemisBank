using ArtemisBank.Core.Application.DTOs.User;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get paginated list of users
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] UserRole? role = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters." });
            }

            var result = await _userService.GetAllAsync(page, pageSize, role);
            return Ok(result);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        /// <summary>
        /// Get users with commerce role
        /// </summary>
        [HttpGet("commerce")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCommerceUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters." });
            }

            var result = await _userService.GetAllAsync(page, pageSize, UserRole.Commerce);
            return Ok(result);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var registered = await _userService.RegisterAsync(
                dto.FirstName,
                dto.LastName,
                dto.Cedula,
                dto.Username,
                dto.Email,
                dto.Password,
                dto.Role.ToString(),
                dto.InitialAmount ?? 0
            );

            if (!registered)
            {
                return BadRequest(new { message = "User could not be created. Please verify the information." });
            }

            return CreatedAtAction(nameof(GetById), new { id = dto.Username }, new { message = "User created successfully. Activation email sent." });
        }

        /// <summary>
        /// Create a commerce user linked to a specific commerce
        /// </summary>
        [HttpPost("commerce/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCommerceUser(int commerceId, [FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Role != UserRole.Commerce)
            {
                return BadRequest(new { message = "User role must be Commerce." });
            }

            var registered = await _userService.RegisterAsync(
                dto.FirstName,
                dto.LastName,
                dto.Cedula,
                dto.Username,
                dto.Email,
                dto.Password,
                UserRole.Commerce.ToString(),
                0
            );

            if (!registered)
            {
                return BadRequest(new { message = "Commerce user could not be created." });
            }

            return CreatedAtAction(nameof(GetById), new { id = dto.Username }, new { message = "Commerce user created successfully. Activation email sent.", commerceId });
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "User ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _userService.UpdateAsync(dto);
            if (!updated)
            {
                return NotFound(new { message = "User not found or could not be updated." });
            }

            return Ok(new { message = "User updated successfully." });
        }

        /// <summary>
        /// Change user status (activate/deactivate)
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeUserStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var success = await _userService.ChangeStatusAsync(dto.AdminId, id, dto.IsActive);
            if (!success)
            {
                return BadRequest(new { message = "Status could not be changed." });
            }

            return Ok(new { message = $"User {(dto.IsActive ? "activated" : "deactivated")} successfully." });
        }
    }
}
