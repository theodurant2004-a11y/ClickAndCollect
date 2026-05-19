using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Cashier : Employee
    {
        private Store cashierStore;
        public Store CashierStore
        {
            get { return cashierStore; }
            set { cashierStore = value; }
        }
        public Cashier()
        {
        }
        public Cashier(int _id, string _firstName, string _surName, string _email, string _password, int _idEmployee, int _storeID)
            : base(_id, _firstName, _surName, _email, _password, _idEmployee, _storeID)
        {
        }
        public static async Task<Cashier> GetCashierAsync(int id, int storeId, IEmployeeDAL employeeDAL)
        {
            Cashier cashier = new Cashier { Id = id };
            cashier.StoreID = storeId; 

            cashier.cashierStore = new Store
            {
                Id = cashier.StoreID 
            };

            return cashier;
        }

        public async Task<List<Order>> GetTodaysOrdersAsync(IStoreDAL storeDAL)
        {
            return await cashierStore.GetTodaysOrdersAsync(storeDAL, this);
        }
    }
}
