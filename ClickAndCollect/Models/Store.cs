using ClickAndCollect.DAL;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    public class Store
    {
        private string name;
        private string roadName;
        private string roadNumber;
        private string city;
        private string postalCode;
        private int id;

        public int Id
        {
            get { return id; }
            init
            {
                if (value <= 0)
                    throw new ArgumentException("ID cannot be negative or zero.");
                id = value;
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                name = value;
            }
        }

        public string RoadName
        {
            get { return roadName; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                roadName = value;
            }
        }

        public string RoadNumber
        {
            get { return roadNumber; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                roadNumber = value;
            }
        }

        public string City
        {
            get { return city; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                city = value;
            }
        }

        public string PostalCode
        {
            get { return postalCode; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                postalCode = value;
            }
        }

        public override string ToString()
        {
            return $"{Name} - {RoadName} {RoadNumber}, {PostalCode} {City}";
        }

        public static async Task<List<Store>> GetStoresAsync(IStoreDAL _storeDAL)
        {
            return await _storeDAL.GetStoresAsync();
        }

        public static async Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(IStoreDAL _storeDAL, Store _store)
        {
            return await _storeDAL.GetAvailableTimeSlotsAsync(_store);
        }

        public static async Task<List<Order>> GetTodaysOrdersAsync(IStoreDAL storeDAL, Cashier cashier)
        {
            return await storeDAL.GetTodaysOrdersAsync(cashier);
        }

        public static async Task<List<Order>> GetOrderToPrepareAsync(IStoreDAL storeDAL, Preparator preparator)
        {
            return await storeDAL.GetOrderToPrepareAsync(preparator);
        }
    }
}