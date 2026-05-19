using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IStoreDAL
    {
        Task<List<Order>> GetTodaysOrdersAsync(Cashier cashier);
    }
}
