using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.PortableExecutable;

namespace ClickAndCollect.DAL
{
    public class EmployeeDAL : IEmployeeDAL
    {
        private readonly string connectionString;
        public EmployeeDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }
        public async Task<Employee> GetEmployeeByEmail(string _email)
        {
            Employee employee = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT 
                            p.personID, 
                            p.firstname, 
                            p.lastname, 
                            p.email, 
                            p.password, 
                            e.employeeID, 
                            e.type, 
                            e.storeID
                        FROM 
                            Person p
                        INNER JOIN Employee e ON p.personID = e.personID
                        Where p.email = @Email;"
                        , con
                    );
                cmd.Parameters.AddWithValue("Email", _email);
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int id = (int)reader["personID"];
                        string? firstName = reader["firstName"].ToString();
                        string? lastName = reader["lastName"].ToString();
                        string? email = reader["email"].ToString();
                        string? password = reader["password"].ToString();
                        int employeeID = (int)reader["employeeID"];
                        string? type = reader["type"].ToString();
                        int storeID = (int)reader["storeID"];

                        if (type == "cashier")
                        {
                            employee = new Cashier(id, firstName, lastName, email, password, employeeID, storeID);
                        }
                        else if (type == "preparator")
                        {
                            employee = new Preparator(id, firstName, lastName, email, password, employeeID, storeID);
                        }
                    }
                }
            }
            return employee;
        }
    }
}
