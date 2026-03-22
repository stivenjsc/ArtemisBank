using ArtemisBank.Core.Application.DTOs.User;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels.Client
{
    public class SelectClientViewModel
    {
        [Required(ErrorMessage = "Average debt is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Debt must be greater than 0")]
        public decimal? AverageDebt { get; set; }

        [Required(ErrorMessage = "At least one client must be selected")]
        public IEnumerable<UserDto>? Clients { get; set; }
        [Required(ErrorMessage = "Please select a client")]
        public string? SelectedClientId { get; set; }

        [Required(ErrorMessage = "ID number (Cedula) is required")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "Cedula must be 123 characters long (including dashes)")]
        [RegularExpression(@"^\d{3}-\d{7}-\d{1}$", ErrorMessage = "Invalid Cedula format. Use XXX-XXXXXXX-X")]
        public string? CurrentCedula { get; set; }
    }
}
