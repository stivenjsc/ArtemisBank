using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.DTOs.CreditCardConsumption;

namespace ArtemisBank.Core.Application.ViewModels.CreditCard
{
    public class CreditCardDetailViewModel
    {
        public CreditCardDto CreditCard { get; set; } = null!;
        public IEnumerable<CreditCardConsumptionDto> Consumptions { get; set; } = [];
    }
}
