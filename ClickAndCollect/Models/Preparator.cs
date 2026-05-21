using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Preparator : Employee
    {
        private Store preparatorStore;

        public Store PreparatorStore
        {
            get { return preparatorStore; }
            set { preparatorStore = value; }
        }

        public Preparator()
        {
        }

        public Preparator(int _id, string _firstName, string _surName, string _email, string _password, int _idEmployee, int _storeID)
            : base(_id, _firstName, _surName, _email, _password, _idEmployee, _storeID)
        {
        }

        public static async Task<Preparator> GetPreparatorAsync(int id, int storeId, IEmployeeDAL employeeDAL)
        {
            Preparator preparator = new Preparator { Id = id };
            preparator.StoreID = storeId;
            preparator.preparatorStore = new Store { Id = storeId };
            return preparator;
        }

        public static async Task<List<Order>> GetOrderToPrepareAsync(IStoreDAL storeDAL, Preparator preparator)
        {
            return await storeDAL.GetOrderToPrepareAsync(preparator);
        }
    }
}