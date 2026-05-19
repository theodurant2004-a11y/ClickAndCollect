using Microsoft.Data.SqlClient;
using System.Data;

namespace ClickAndCollect.DAL
{
    public class CategoryDAL : ICategoryDAL
    {
        private readonly string connectionString;

        public CategoryDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            List<string> categories = new List<string>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT name FROM Category"
                        , con
                    );
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string nameCategory = reader.GetString("name");
                        categories.Add(nameCategory);
                    }
                }
            }
            return categories;
        }
    }
}
