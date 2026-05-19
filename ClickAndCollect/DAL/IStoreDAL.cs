using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IStoreDAL
    {
        Task<List<Order>> GetTodaysOrdersAsync(Cashier cashier);
        Task<List<Store>> GetStoresAsync();
        Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(Store _store);
        Task<List<Order>> GetOrderToPrepareAsync(Preparator preparator);
    }
}
