using Microsoft.Data.SqlClient;
using ClickAndCollect.Models;
using ClickAndCollect.DAL;

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

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            List<Article> articles = new List<Article>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT articleID, a.name AS nameProduct, price, description, c.name AS nameCategory 
                          FROM Article a
	                        JOIN Category c ON a.categoryID = c.categoryID"
                        , con
                    );
                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int idArticle = reader.GetInt32(0);
                        string nameProduct = reader.GetString(1);
                        decimal price = reader.GetDecimal(2);
                        string description = reader.GetString(3);
                        string nameCategory = reader.GetString(4);

                        Article a = new Article(idArticle, nameProduct, price, description, new Category(nameCategory));
                        articles.Add(a);
                    }
                }
            }
            return articles;
        }
    }
}
