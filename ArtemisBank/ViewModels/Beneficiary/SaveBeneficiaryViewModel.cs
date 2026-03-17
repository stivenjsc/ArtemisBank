using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.ViewModels.Beneficiary
{
    public class SaveBeneficiaryViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido.")]
        [StringLength(9, ErrorMessage = "El número de cuenta debe tener {1} caracteres.", MinimumLength = 9)]
        public string AccountNumber { get; set; } = string.Empty;

        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
