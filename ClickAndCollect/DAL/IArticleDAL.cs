using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IArticleDAL
    {
        Task<List<Article>> GetArticlesAsync(List<int>? ids);
    }
}
