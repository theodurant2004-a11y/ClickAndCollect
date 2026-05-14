using System.Security.Cryptography;

namespace ClickAndCollect.Models
{
    public class Employee : User
    {
        private int idEmployee;
        private int storeID;

        public int StoreID
        {
            get { return storeID; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Store ID must be a positive integer.");
                storeID = value;
            }
        }

        public int IdEmployee
        {
            get { return idEmployee; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Employee ID must be a positive integer.");
                idEmployee = value;
            }
        }
        public Employee()
        {
        }
        public Employee(int _id, string _firstName, string _surName, string _email, string _password, int _idEmployee, int _storeID)
            : base(_id, _firstName, _surName, _email, _password)
        {
            IdEmployee = _idEmployee;
            StoreID = _storeID;
        }
    }
}
