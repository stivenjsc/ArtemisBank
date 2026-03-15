using ArtemisBank.Core.Application.DTOs.Dashboard;

namespace ArtemisBank.Core.Application.Interfaces.IServices
{
    public interface IDashboardService
    {
        Task<DashboardAdminDto> GetAdminDashboardAsync();
        Task<DashboardClientDto> GetClientDashboardAsync(string clientId);
    }
}
