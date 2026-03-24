using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.WebAPI.DTOs.User
{
    public class ChangeUserStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public bool Status { get; set; }
    }
}
