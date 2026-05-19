using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface ICategoryDAL
    {
        Task<List<Category>> GetCategoriesAsync();
    }
}
