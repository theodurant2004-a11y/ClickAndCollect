using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IEmployeeDAL
    {
        Task<Employee> GetEmployeeByEmail(string _email);
    }
}
