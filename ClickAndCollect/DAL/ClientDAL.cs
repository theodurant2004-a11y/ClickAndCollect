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
                        @"SELECT 
                                p.personID, 
                                firstname, 
                                lastname, 
                                email, 
                                password, 
                                phoneNumber, 
                                roadName,
                                roadNumber,
                                postalCode,
                                cityName
                            FROM Person p
                                JOIN Client c ON p.personID = c.personID
                                JOIN Address a ON p.addressID = a.addressID
                                JOIN PostalCode pc ON a.postalCodeID = pc.postalCodeID
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
                        string roadName = reader.GetString("roadName");
                        string roadNumber = reader.GetString("roadNumber");
                        string postalCode = reader.GetInt32("postalCode").ToString();
                        string cityName = reader.GetString("cityName");

                        client = new(id, firstName, lastName, email, password, phone, roadName, roadNumber, cityName, postalCode);
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
                SqlCommand cmd = new SqlCommand(
                    @"DECLARE @PostalCodeID INT;
                      SELECT @PostalCodeID = postalCodeID 
                      FROM PostalCode 
                      WHERE postalCode = @PostalCode AND cityName = @CityName;

                      IF @PostalCodeID IS NULL
                      BEGIN
                          INSERT INTO PostalCode (postalCode, cityName)
                          VALUES (@PostalCode, @CityName);
                          SET @PostalCodeID = SCOPE_IDENTITY();
                      END

              
                      INSERT INTO Address (roadName, roadNumber, postalCodeID)
                      VALUES (@RoadName, @RoadNumber, @PostalCodeID);
                      DECLARE @AddressID INT = SCOPE_IDENTITY();

             
                      INSERT INTO Person (firstname, lastname, email, password, addressID)
                      VALUES (@FirstName, @LastName, @Email, @Password, @AddressID);
                      DECLARE @PersonID INT = SCOPE_IDENTITY();

             
                      INSERT INTO Client (personID, phoneNumber)
                      VALUES (@PersonID, @PhoneNumber);

                      SELECT @PersonID;", con
                    );

                cmd.Parameters.AddWithValue("@FirstName", _client.FirstName);
                cmd.Parameters.AddWithValue("@LastName", _client.SurName);
                cmd.Parameters.AddWithValue("@Email", _client.Email);
                cmd.Parameters.AddWithValue("@Password", _client.Password);
                cmd.Parameters.AddWithValue("@PhoneNumber", _client.Phone);
                cmd.Parameters.AddWithValue("@RoadName", _client.RoadName);
                cmd.Parameters.AddWithValue("@RoadNumber", _client.RoadNumber);
                cmd.Parameters.AddWithValue("@PostalCode", _client.PostalCode);
                cmd.Parameters.AddWithValue("@CityName", _client.City);

                await con.OpenAsync();
                id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            return id;
        }

        public async Task<int> UpdateClientInfo(int? _id, Client _client)
        {
            int rowsAffected = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"DECLARE @PostalCodeID INT;
                      SELECT @PostalCodeID = postalCodeID 
                      FROM PostalCode 
                      WHERE postalCode = @PostalCode AND cityName = @CityName;

                      IF @PostalCodeID IS NULL
                      BEGIN
                          INSERT INTO PostalCode (postalCode, cityName)
                          VALUES (@PostalCode, @CityName);
                          SET @PostalCodeID = SCOPE_IDENTITY();
                      END

                      UPDATE Address
                      SET roadName = @RoadName,
                          roadNumber = @RoadNumber,
                          postalCodeID = @PostalCodeID
                      FROM Address a
                          JOIN Person p ON p.addressID = a.addressID
                      WHERE p.personID = @PersonID;

                      UPDATE Person
                      SET firstname = @FirstName,
                          lastname = @LastName
                      WHERE personID = @PersonID;

                      UPDATE Client
                      SET phoneNumber = @PhoneNumber
                      WHERE personID = @PersonID;"
                    , con);

                cmd.Parameters.AddWithValue("@PersonID", _id);
                cmd.Parameters.AddWithValue("@FirstName", _client.FirstName);
                cmd.Parameters.AddWithValue("@LastName", _client.SurName);
                cmd.Parameters.AddWithValue("@PhoneNumber", _client.Phone);
                cmd.Parameters.AddWithValue("@RoadName", _client.RoadName);
                cmd.Parameters.AddWithValue("@RoadNumber", _client.RoadNumber);
                cmd.Parameters.AddWithValue("@PostalCode", _client.PostalCode);
                cmd.Parameters.AddWithValue("@CityName", _client.City);

                await con.OpenAsync();
                rowsAffected = await cmd.ExecuteNonQueryAsync();
            }
            return rowsAffected;
        }
    }
}
