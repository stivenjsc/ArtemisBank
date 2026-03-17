using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.ViewModels.User
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [DataType(DataType.Text)]
        public string Username { get; set; } = string.Empty;

        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
