using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.DTOs.CreditCardConsumption;

namespace ArtemisBank.Core.Application.ViewModels.Account
{
    public class CardDetailViewModel
    {
        public IEnumerable<CreditCardConsumptionDto> Consumptions { get; set; } = new List<CreditCardConsumptionDto>();
        public CreditCardDto CreditCard { get; set; } = new CreditCardDto();
    }
}
