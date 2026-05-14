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
        private readonly IEmployeeDAL employeeDAL;

        public HomeController(IArticleDAL _articleDAL, ICategoryDAL _categoryDAL, IClientDAL _clientDAL, IEmployeeDAL _employeeDAL)
        {
            articleDAL = _articleDAL;
            categoryDAL = _categoryDAL;
            clientDAL = _clientDAL;
            employeeDAL = _employeeDAL;
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

        //Sign Up
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

        //Connexion
        public IActionResult Connexion()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Connexion(string email, string password)
        {
            var client = await clientDAL.GetClientByEmail(email);

            if(client != null)
            {
                if(password == client.Password)
                {
                    HttpContext.Session.SetInt32("Id", client.Id);
                    HttpContext.Session.SetString("Email", client.Email);
                    HttpContext.Session.SetString("FirstName", client.FirstName);
                    HttpContext.Session.SetString("Type", "Client");

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid email or password";
                    return View();
                }
            }

            var employee = await employeeDAL.GetEmployeeByEmail(email);
            if(employee != null)
            {
                if (password == employee.Password)
                {
                    HttpContext.Session.SetInt32("Id", employee.Id);
                    HttpContext.Session.SetString("Email", employee.Email);
                    HttpContext.Session.SetString("FirstName", employee.FirstName);
                    if (employee is Cashier)
                    {
                        HttpContext.Session.SetString("Type", "Cashier");
                        return RedirectToAction("IndexCashier");
                    }
                    else if (employee is Preparator)
                    {
                        HttpContext.Session.SetString("Type", "Preparator");
                        return RedirectToAction("IndexPreparator");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid email or password";
                    return View();
                }
            }
            ViewBag.ErrorMessage = "This user doesn't exist";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Connexion");
        }
        public IActionResult IndexCashier()
        {
            if (HttpContext.Session.GetString("Type") != "Cashier")
            {
                return RedirectToAction("Connexion");
            }
            return View();
        }
        public IActionResult IndexPreparator()
        {
            // Sécurité
            if (HttpContext.Session.GetString("Type") != "Preparator")
            {
                return RedirectToAction("Connexion");
            }
            return View();
        }

        public IActionResult Profile()
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
