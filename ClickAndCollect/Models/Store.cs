using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Store
    {
        private int storeID;
        private string? name;
        private string? address;

        public string Name
        {
            get { return name; } 
            set { ArgumentNullException.ThrowIfNull(value); name = value; }
        }
        public string Address
        {
            get { return address; }
            set { ArgumentNullException.ThrowIfNull(value); address = value; }
        }
        public int StoreID
        {
            get { return storeID; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Store ID cannot be negative or zero.");
                storeID = value;
            }
        }
        public Store() { }
        public Store(int _storeID, string _name, string _adresse)
        {
            StoreID = _storeID;
            Name = _name;
            Address = _adresse;
        }

        public async Task<List<Order>> GetTodaysOrdersAsync(IStoreDAL storeDAL, Cashier cashier)
        {
            return await storeDAL.GetTodaysOrdersAsync(cashier);
        }
    }
}
