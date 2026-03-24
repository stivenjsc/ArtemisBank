using ArtemisBank.Core.Application.DTOs.Commerce;
using ArtemisBank.Core.Application.Interfaces.IServices;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    public class CommerceController : BaseApiController
    {
        private readonly ICommerceService _commerceService;

        public CommerceController(ICommerceService commerceService)
        {
            _commerceService = commerceService;
        }

        /// <summary>
        /// Get paginated list of commerces
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters." });
            }

            var result = await _commerceService.GetAllPagedAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get commerce by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var commerce = await _commerceService.GetByIdAsync(id);
            if (commerce == null)
            {
                return NotFound(new { message = "Commerce not found." });
            }
            return Ok(commerce);
        }

        /// <summary>
        /// Create a new commerce
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CommerceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _commerceService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Update commerce information
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CommerceDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "Commerce ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCommerce = await _commerceService.GetByIdAsync(id);
            if (existingCommerce == null)
            {
                return NotFound(new { message = "Commerce not found." });
            }

            await _commerceService.UpdateAsync(dto);
            return Ok(new { message = "Commerce updated successfully." });
        }

        /// <summary>
        /// Toggle commerce active status
        /// </summary>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var commerce = await _commerceService.GetByIdAsync(id);
            if (commerce == null)
            {
                return NotFound(new { message = "Commerce not found." });
            }

            if (!commerce.IsActive)
            {
                var hasActiveUser = await _commerceService.CommerceHasActiveUserAsync(id);
                if (!hasActiveUser)
                {
                    return BadRequest(new { message = "Cannot activate commerce without an active associated user." });
                }
            }

            commerce.IsActive = !commerce.IsActive;
            await _commerceService.UpdateAsync(commerce);

            return Ok(new { message = $"Commerce {(commerce.IsActive ? "activated" : "deactivated")} successfully.", isActive = commerce.IsActive });
        }
    }
}
