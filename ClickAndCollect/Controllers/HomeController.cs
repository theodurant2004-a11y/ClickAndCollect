using ClickAndCollect.DAL;
using ClickAndCollect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClickAndCollect.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleDAL articleDAL;
        private readonly ICategoryDAL categoryDAL;
        private readonly IClientDAL clientDAL;
        private readonly IEmployeeDAL employeeDAL;
        private readonly IStoreDAL storeDAL;

        public HomeController(IArticleDAL _articleDAL, ICategoryDAL _categoryDAL, IClientDAL _clientDAL, IEmployeeDAL _employeeDAL, IStoreDAL _storeDAL)
        {
            articleDAL = _articleDAL;
            categoryDAL = _categoryDAL;
            clientDAL = _clientDAL;
            employeeDAL = _employeeDAL;
            storeDAL = _storeDAL;
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
        public async Task<IActionResult> Connexion(Client loginInfo)
        {
            var client = await clientDAL.GetClientByEmail(loginInfo.Email);

            if(client != null)
            {
                if(loginInfo.Password == client.Password)
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

            var employee = await employeeDAL.GetEmployeeByEmail(loginInfo.Email);
            if(employee != null)
            {
                if (loginInfo.Password == employee.Password)
                {
                    HttpContext.Session.SetInt32("Id", employee.Id);
                    HttpContext.Session.SetString("Email", employee.Email);
                    HttpContext.Session.SetString("FirstName", employee.FirstName);
                    HttpContext.Session.SetInt32("CashierId", employee.Id);
                    HttpContext.Session.SetInt32("storeId", employee.StoreID);
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
            ViewBag.ErrorMessage = "Invalid email or password";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Connexion");
        }
        public async Task<IActionResult> IndexCashier()
        {
            if (HttpContext.Session.GetString("Type") != "Cashier" ||
                HttpContext.Session.GetInt32("CashierId") == null ||
                HttpContext.Session.GetInt32("storeId") == null)
            {
                return RedirectToAction("Connexion");
            }

            int? cashierID = HttpContext.Session.GetInt32("CashierId");
            int? storeId = HttpContext.Session.GetInt32("storeId");

            Cashier currentCashier = new Cashier
            {
                Id = cashierID.Value,
                StoreID = storeId.Value
            };

            try
            {
                List<Order> todaysOrders = await storeDAL.GetTodaysOrdersAsync(currentCashier);

                if (todaysOrders == null)
                {
                    ViewBag.InfoMessage = "No orders to be fulfilled today.";
                }
                if (todaysOrders.Count == 0)
                {
                    ViewBag.InfoMessage = "No orders to be 0 today.";
                }

                return View(todaysOrders);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Une erreur est survenue lors de la récupération des commandes : " + ex.Message;
                return View(new List<Order>());
            }
        }
        public async Task<IActionResult> IndexPreparator()
        {
            // Sécurité
            if (HttpContext.Session.GetString("Type") != "Preparator")
            {
                return RedirectToAction("Connexion");
            }

            


            return View();
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
