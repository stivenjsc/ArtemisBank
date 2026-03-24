using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.WebAPI.DTOs.Commerce
{
    public class ChangeCommerceStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public bool Status { get; set; }
    }
}
