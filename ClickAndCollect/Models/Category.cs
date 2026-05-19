using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Category
    {
        private string name;

        public Category(string _name)
        {
            Name = _name;
        }

        public string Name
        {
            get { return name; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                name = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public static async Task<List<Category>> GetCategoriesAsync(ICategoryDAL _dAL)
        {
            return await _dAL.GetCategoriesAsync();
        }
    }
}
