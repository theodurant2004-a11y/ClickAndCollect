namespace ClickAndCollect.DAL
{
    public interface ICategoryDAL
    {
        Task<List<string>> GetCategoriesAsync();
    }
}
