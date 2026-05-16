namespace ClickAndCollect.Models
{
    public class Cashier : Employee
    {
        public Cashier()
        {
        }
        public Cashier(int _id, string _firstName, string _surName, string _email, string _password, int _idEmployee, int _storeID)
            : base(_id, _firstName, _surName, _email, _password, _idEmployee, _storeID)
        {
        }
    }
}
