namespace ArtemisBank.Core.Application.ViewModels.Cashier
{
    public class CashierDepositViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public string Error { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
