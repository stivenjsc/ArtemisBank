using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    public class LoanController : BaseApiController
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// Get paginated list of loans
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] LoanStatus? status = null,
            [FromQuery] string? cedula = null)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters." });
            }

            var result = await _loanService.GetAllPagedAsync(page, pageSize, status, cedula);
            return Ok(result);
        }

        /// <summary>
        /// Get loan details with amortization table
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
            {
                return NotFound(new { message = "Loan not found." });
            }
            return Ok(loan);
        }

        /// <summary>
        /// Assign a new loan to a client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Assign([FromBody] AssignLoanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if client already has an active loan
            var hasActiveLoan = await _loanService.ClientHasActiveLoanAsync(dto.ClientId);
            if (hasActiveLoan)
            {
                return BadRequest(new { message = "Client already has an active loan." });
            }

            // Evaluate client risk
            var riskEvaluation = await _loanService.EvaluateRiskAsync(
                dto.ClientId, 
                dto.Amount, 
                dto.AnnualInterestRate, 
                dto.TermInMonths);

            if (riskEvaluation.IsHighRisk)
            {
                return Conflict(new
                {
                    message = "Client is considered high risk. Their debt will exceed the system's average threshold.",
                    averageDebt = riskEvaluation.AverageDebt,
                    currentDebt = riskEvaluation.CurrentDebt,
                    requiresConfirmation = true
                });
            }

            var loan = await _loanService.AssignAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        /// <summary>
        /// Assign loan with high risk confirmation
        /// </summary>
        [HttpPost("confirm-high-risk")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AssignWithRiskConfirmation([FromBody] AssignLoanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hasActiveLoan = await _loanService.ClientHasActiveLoanAsync(dto.ClientId);
            if (hasActiveLoan)
            {
                return BadRequest(new { message = "Client already has an active loan." });
            }

            var loan = await _loanService.AssignAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        /// <summary>
        /// Update loan interest rate
        /// </summary>
        [HttpPatch("{id}/rate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRate(int id, [FromBody] UpdateLoanRateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
            {
                return NotFound(new { message = "Loan not found." });
            }

            await _loanService.UpdateInterestRateAsync(id, dto.AnnualInterestRate);
            return NoContent();
        }
    }
}
