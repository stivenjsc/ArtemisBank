namespace ArtemisBank.WebAPI.DTOs.Loan
{
    public record AssignLoanApiDto(string ClientId, decimal Amount, decimal AnnualRate, int MonthsInstallments);
}
