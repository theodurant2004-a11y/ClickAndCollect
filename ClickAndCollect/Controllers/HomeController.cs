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
        private readonly IClientDAL clientDAL;

        public HomeController(IArticleDAL _articleDAL, ICategoryDAL _categoryDAL, IClientDAL _clientDAL)
        {
            articleDAL = _articleDAL;
            categoryDAL = _categoryDAL;
            clientDAL = _clientDAL;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(Client user)
        {
            if(!ModelState.IsValid)
            {
                return View(user);
            }
            //verfier si le mail existe en DB
            if(await Client.GetClientByEmail(clientDAL, user.Email) != null)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(user);
            }
            // ajouter le user en DB et récuperer son id pour session
            int clientId = await user.AddClientAsync(clientDAL, user);
            if(clientId == -1)
            {
                //erreur pendant la création du compte
            }
            HttpContext.Session.SetInt32("Id", clientId);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("FirstName", user.FirstName);
            HttpContext.Session.SetString("Type", "Client");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Profile()
        {
            if(HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to view your profile."; 
                return RedirectToAction("SignUp");
            }
            string email = HttpContext.Session.GetString("Email");
            Client user = await Client.GetClientByEmail(clientDAL, email);
            return View(user);
        }

        public async Task<IActionResult> EditProfile()
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to edit your profile.";
                return RedirectToAction("SignUp");
            }
            string email = HttpContext.Session.GetString("Email");
            Client user = await Client.GetClientByEmail(clientDAL, email);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Client user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            int? clientId = HttpContext.Session.GetInt32("Id");
            //modification du user en DB
            int rows = await clientDAL.UpdateClientInfo(clientId, user);

            if(rows == 0)
            {
                //erreur pendant la modification du compte
            }

            HttpContext.Session.SetString("FirstName", user.FirstName);
            return RedirectToAction("Profile");
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
