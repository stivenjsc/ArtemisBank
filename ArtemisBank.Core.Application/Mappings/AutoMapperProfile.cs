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
using ArtemisBank.Core.Domain.Enums;

namespace ArtemisBank.Core.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == AccountStatus.Active))
                .ReverseMap();
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();

            #region Loan Mapping
            CreateMap<Loan, LoanDto>()
            .ForMember(dest => dest.TotalInstallments, opt => opt.MapFrom(src => src.Installments.Count))
            .ForMember(dest => dest.PaidInstallments, opt => opt.MapFrom(src => src.Installments
                .Count(x => x.AmountPaid >= x.InstallmentAmount)))
            .ForMember(dest => dest.PendingAmount, opt => opt.MapFrom(src => src.Installments
                .Where(x => x.AmountPaid < x.InstallmentAmount).Sum(x => x.InstallmentAmount - x.AmountPaid)))
            .ForMember(dest => dest.IsOnTime, opt => opt.MapFrom(src => !src.Installments.Any(x => x.IsOverdue)));
            #endregion

            CreateMap<Transaction, TransactionDto>().ReverseMap();
            CreateMap<Beneficiary, BeneficiaryDto>().ReverseMap();
            CreateMap<Commerce, CommerceDto>().ReverseMap();
            CreateMap<CreditCardConsumption, CreditCardConsumptionDto>().ReverseMap();
            CreateMap<LoanInstallment, LoanInstallmentDto>().ReverseMap();
        }
    }
}
