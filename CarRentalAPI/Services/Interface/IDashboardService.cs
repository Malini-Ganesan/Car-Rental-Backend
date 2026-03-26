namespace CarRentalAPI.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetDashboardDataAsync();
    }
}