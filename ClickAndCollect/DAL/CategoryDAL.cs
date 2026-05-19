using ClickAndCollect.Models;
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

        public async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> categories = new List<Category>();

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
                        Category category = new(nameCategory);

                        categories.Add(category);
                    }
                }
            }
            return categories;
        }
    }
}
