using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels.Beneficiary
{
    public class SaveBeneficiaryViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido.")]
        [StringLength(12, ErrorMessage = "El número de cuenta no es válido.", MinimumLength = 9)]
        public string AccountNumber { get; set; } = string.Empty;

        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
