using ClickAndCollect.DAL;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClickAndCollect.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleDAL articleDAL;
        private readonly ICategoryDAL categoryDAL;

        public HomeController(IArticleDAL _articleDAL, ICategoryDAL _categoryDAL)
        {
            articleDAL = _articleDAL;
            categoryDAL = _categoryDAL;
        }

        public async Task<IActionResult> Index(string? category)
        {
            List<Article> articles = await Article.GetAllArticlesAsync(articleDAL);
            List<string> categories = await Category.GetCategoriesAsync(categoryDAL);

            if (!string.IsNullOrEmpty(category))
            {
                articles = articles.Where(a => a.Category.ToString() == category).ToList();
            }

            ViewBag.Categories = categories;
            return View(articles);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
