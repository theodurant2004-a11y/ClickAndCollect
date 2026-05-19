using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Article
    {
        private int idArticle;
        private string nameProduct;
        private decimal price;
        private string description;
        private Category category;

        public Article(int _idArticle, string _nameProduct, decimal _price, string _description, Category _category)
        {
            IDArticle = _idArticle;
            NameProduct = _nameProduct;
            Price = _price;
            Description = _description;
            Category = _category;
        }

        public Category Category
        {
            get { return category; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                category = value;
            }
        }


        public string Description
        {
            get { return description; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                description = value;
            }
        }


        public decimal Price
        {
            get { return price; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Price cannot be negative or zero.");
                }
                price = value;
            }
        }

        public string NameProduct
        {
            get { return nameProduct; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                nameProduct = value;
            }
        }

        public int IDArticle
        {
            get { return idArticle; }
            init
            {
                if (value <= 0)
                {
                    throw new ArgumentException("ID cannot be negative.");
                }
                idArticle = value;
            }
        }

        public static async Task<List<Article>> GetAllArticlesAsync(IArticleDAL _dAL, List<int>? ids = null)
        {
            return await _dAL.GetArticlesAsync(ids);
        }

        public override string ToString()
        {
            return $"{IDArticle} : {NameProduct} {Price}, {Category}. Description : {Description}";
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return this.ToString() == obj.ToString();
        }
    }
}
