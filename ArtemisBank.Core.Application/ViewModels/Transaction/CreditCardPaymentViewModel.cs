using System.ComponentModel.DataAnnotations;
using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.DTOs.SavingsAccount;

namespace ArtemisBank.Core.Application.ViewModels.Transaction
{
    public class CreditCardPaymentViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es requerida.")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tarjeta de crédito es requerida.")]
        public string CreditCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Amount { get; set; }

        public IEnumerable<SavingsAccountDto> UserAccounts { get; set; } = [];
        public IEnumerable<CreditCardDto> UserCreditCards { get; set; } = [];

        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
