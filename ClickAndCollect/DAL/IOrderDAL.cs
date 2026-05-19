using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IOrderDAL
    {
        Task<int> PlaceOrderAsync(Client client, Store store, TimeSlot timeSlot, Dictionary<int, int> cart);
    }
}