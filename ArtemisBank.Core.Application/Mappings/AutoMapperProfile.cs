using AutoMapper;
using ArtemisBank.Core.Application.DTOs.Beneficiary;
using ArtemisBank.Core.Application.DTOs.Commerce;
using ArtemisBank.Core.Application.DTOs.CreditCard;
using ArtemisBank.Core.Application.DTOs.CreditCardConsumption;
using ArtemisBank.Core.Application.DTOs.Loan;
using ArtemisBank.Core.Application.DTOs.LoanInstallment;
using ArtemisBank.Core.Application.DTOs.SavingsAccount;
using ArtemisBank.Core.Application.DTOs.Transaction;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDto>().ReverseMap();
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();
            CreateMap<Loan, LoanDto>().ReverseMap();
            CreateMap<Transaction, TransactionDto>().ReverseMap();
            CreateMap<Beneficiary, BeneficiaryDto>().ReverseMap();
            CreateMap<Commerce, CommerceDto>().ReverseMap();
            CreateMap<CreditCardConsumption, CreditCardConsumptionDto>().ReverseMap();
            CreateMap<LoanInstallment, LoanInstallmentDto>().ReverseMap();
        }
    }
}
