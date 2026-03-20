namespace ArtemisBank.Core.Application.ViewModels.Cashier
{
    public class CashierThirdPartyTransferViewModel
    {
        public string SourceAccountNumber { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public string Error { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationHolderName { get; set; } = string.Empty;
    }
}
