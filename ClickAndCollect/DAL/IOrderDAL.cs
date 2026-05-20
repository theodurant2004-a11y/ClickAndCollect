using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IOrderDAL
    {
        Task<int> PlaceOrderAsync(Client client, Store store, TimeSlot timeSlot, Dictionary<int, int> cart);
        Task<int> FinalizeOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<int> RemoveItemFromOrderAsync(int orderId, int articleId);
        Task<int> PrepareOrderAsync(Order order);
    }
}