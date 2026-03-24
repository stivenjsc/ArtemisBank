using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    public class CreditCardController : BaseApiController
    {
        private readonly ICreditCardService _creditCardService;

        public CreditCardController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        /// <summary>
        /// Get paginated list of credit cards
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] CardStatus? status = null,
            [FromQuery] string? cedula = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters." });
            }

            var result = await _creditCardService.GetAllPagedAsync(page, pageSize, status, cedula);
            return Ok(result);
        }

        /// <summary>
        /// Get credit card details with consumption history
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Credit card not found." });
            }
            return Ok(card);
        }

        /// <summary>
        /// Assign a new credit card to a client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Assign([FromBody] AssignCreditCardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var card = await _creditCardService.AssignAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = card.Id }, card);
        }

        /// <summary>
        /// Update credit card limit
        /// </summary>
        [HttpPatch("{id}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] UpdateCreditLimitDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Credit card not found." });
            }

            // Validate new limit is not less than current debt
            if (dto.CreditLimit < card.AmountOwed)
            {
                return BadRequest(new { message = "New credit limit cannot be less than current debt." });
            }

            await _creditCardService.UpdateLimitAsync(id, dto.CreditLimit);
            return NoContent();
        }

        /// <summary>
        /// Cancel a credit card
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Cancel(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Credit card not found." });
            }

            // Validate card has no pending debt
            if (card.AmountOwed > 0)
            {
                return BadRequest(new { message = "Cannot cancel card with pending debt. Client must pay the full balance first." });
            }

            await _creditCardService.CancelAsync(id);
            return NoContent();
        }
    }
}
