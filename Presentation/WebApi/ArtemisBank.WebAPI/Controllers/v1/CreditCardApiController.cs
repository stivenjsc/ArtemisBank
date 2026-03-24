using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.Interfaces.IServices;
using ArtemisBank.Core.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    public class CreditCardApiController( ICreditCardService creditCardService, ICreditCardConsumptionService consumptionService) : BaseApiController
    {
        private readonly ICreditCardService _creditCardService = creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService = consumptionService;

        
    }
}
