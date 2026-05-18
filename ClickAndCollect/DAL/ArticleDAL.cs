using ClickAndCollect.DAL;
using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClickAndCollect.DAL
{
    public class ArticleDAL : IArticleDAL
    {
        Article article = null;
        private readonly string connectionString;

        public ArticleDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public async Task<List<Article>> GetArticlesAsync(List<int>? ids = null)
        {
            List<Article> articles = new List<Article>();

            string query = "SELECT a.articleID, a.name, a.price, a.description, c.name AS categoryName " +
                           "FROM Article a JOIN Category c ON a.categoryID = c.categoryID";

            if (ids != null && ids.Count > 0)
                query += $" WHERE a.articleID IN ({string.Join(",", ids)})";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32("articleID");
                        string name = reader.GetString("name");
                        decimal price = reader.GetDecimal("price");
                        string description = reader.GetString("description");
                        string categoryName = reader.GetString("categoryName");
                        articles.Add(new Article(id, name, price, description, new Category(categoryName)));
                    }
                }
            }
            return articles;
        }
    }
}
