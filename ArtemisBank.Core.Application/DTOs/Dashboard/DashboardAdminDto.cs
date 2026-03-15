namespace ArtemisBank.Core.Application.DTOs.Dashboard
{
    public class DashboardAdminDto
    {
        public int TotalTransactions { get; set; }
        public int TotalActiveTransactions { get; set; }
        public int TotalInactiveTransactions { get; set; }
        public decimal TotalDailyPayments { get; set; }
        public int TotalAssignedProducts { get; set; }
        public int TotalActiveClients { get; set; }
        public int TotalInactiveClients { get; set; }
    }
}
