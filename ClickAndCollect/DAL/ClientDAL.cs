using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.PortableExecutable;

namespace ClickAndCollect.DAL
{
    public class ClientDAL : IClientDAL
    {
        private readonly string connectionString;

        public ClientDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public async Task<Client> GetClientByEmail(string _email)
        {
            Client client = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT * 
                        FROM Person p
	                        JOIN Client c ON p.personID = c.personID
                            WHERE email = @Email;"
                        , con
                    );
                cmd.Parameters.AddWithValue("Email", _email);
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32("personID");
                        string firstName = reader.GetString("firstName");
                        string lastName = reader.GetString("lastName");
                        string email = reader.GetString("email");
                        string password = reader.GetString("password");
                        string phone = reader.GetString("phoneNumber");
                        //string address = reader.GetString("address"); il faut regarder pour l'adresse

                        client = new(id, firstName, lastName, email, password, phone, "address");
                    }
                }
            }
            return client;
        }

        public async Task<int> AddClientAsync(Client _client)
        {
            int id = -1;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"INSERT INTO Person (firstname, lastname, email, password, addressID)
                          VALUES (@FirstName, @LastName, @Email, @Password, 1);

                          DECLARE @PersonID INT = SCOPE_IDENTITY();

                          INSERT INTO Client (personID, phoneNumber)
                          VALUES (@PersonID, @PhoneNumber);

                          SELECT @PersonID", con
                    );
                cmd.Parameters.AddWithValue("@FirstName", _client.FirstName);
                cmd.Parameters.AddWithValue("@LastName", _client.SurName);
                cmd.Parameters.AddWithValue("@Email", _client.Email);
                cmd.Parameters.AddWithValue("@Password", _client.Password);
                cmd.Parameters.AddWithValue("@PhoneNumber", _client.Phone);

                await con.OpenAsync();
                id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            return id;
        }
    }
}
