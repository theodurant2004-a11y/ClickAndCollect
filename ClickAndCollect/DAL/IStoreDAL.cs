using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IStoreDAL
    {
        Task<List<Order>> GetTodaysOrdersAsync(Cashier cashier);
        Task<List<Order>> GetOrderToPrepareAsync(Preparator preparator);
        Task<Order> GetOrderByIdAsync(int orderId);
    }
}
