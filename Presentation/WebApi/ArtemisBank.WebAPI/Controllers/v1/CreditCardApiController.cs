using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.WebAPI.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class CreditCardApiController( ICreditCardService creditCardService, ICreditCardConsumptionService consumptionService) : BaseApiController
    {
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService = consumptionService;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? cedula = null,
            [FromQuery] string? estado = null,
            [FromQuery] int pagina = 1)
        {
            CardStatus? status = null;
            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<CardStatus>(estado, out var parsed))
                status = parsed;

            var result = await _creditCardService.GetAllPagedAsync(pagina, 20, status, cedula);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Assign([FromBody] AssignCreditCardDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _creditCardService.AssignAsync(dto);
            return StatusCode(201, new { message = "Card assigned correctly." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound(new { message = "Card not found." });

            var consumptions = await _consumptionService.GetByCardIdAsync(id);
            return Ok(new { consumos = consumptions });
        }

        [HttpPatch("{id}/limit")]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] UpdateLimitRequest request)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound(new { message = "Card not found.." });

            if (request.NewLimit < card.AmountOwed)
                return BadRequest(new { message = "The new limit cannot be less than the current debt." });

            await _creditCardService.UpdateLimitAsync(id, request.NewLimit);
            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return NotFound(new { message = "Card not found." });

            if (card.AmountOwed > 0)
                return BadRequest(new { message = "The client still has outstanding debts." });

            await _creditCardService.CancelAsync(id);
            return NoContent();
        }
    }
}
