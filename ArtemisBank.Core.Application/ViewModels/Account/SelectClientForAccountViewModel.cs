using ArtemisBank.Core.Application.DTOs.User;

namespace ArtemisBank.Core.Application.ViewModels.Account
{
    public class SelectClientForAccountViewModel
    {
        public string? SelectedClientId { get; set; }

        public IEnumerable<UserDto> Clients { get; set; } = [];
        public string? CurrentCedula { get; set; }
    }
}
