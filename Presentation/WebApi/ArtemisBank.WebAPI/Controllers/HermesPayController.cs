using ArtemisBank.Core.Application.DTOs.Payment;
using ArtemisBank.Core.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers
{
    [Route("pay")]
    [ApiController]
    public class HermesPayController : ControllerBase
    {
        private readonly IPaymentProcessorService _paymentProcessor;

        public HermesPayController(IPaymentProcessorService paymentProcessor)
        {
            _paymentProcessor = paymentProcessor;
        }

        /// <summary>
        /// Get all transactions for a specific commerce
        /// </summary>
        [HttpGet("get-transactions/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactions(int commerceId)
        {
            var transactions = await _paymentProcessor.GetCommerceTransactionsAsync(commerceId);
            
            if (!transactions.Any())
            {
                return NotFound(new { message = "No transactions found for this commerce." });
            }

            return Ok(transactions);
        }

        /// <summary>
        /// Process a payment for a commerce
        /// </summary>
        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (paymentDto.TransactionAmount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero." });
            }

            var result = await _paymentProcessor.ProcessPaymentAsync(commerceId, paymentDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
