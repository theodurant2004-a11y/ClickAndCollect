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
        private readonly IOrderDAL orderDAL;

        public HomeController(IArticleDAL _articleDAL, ICategoryDAL _categoryDAL, IClientDAL _clientDAL, IEmployeeDAL _employeeDAL, IStoreDAL _storeDAL, IOrderDAL _orderDAL)
        {
            articleDAL = _articleDAL;
            categoryDAL = _categoryDAL;
            clientDAL = _clientDAL;
            employeeDAL = _employeeDAL;
            storeDAL = _storeDAL;
            orderDAL = _orderDAL;
        }

        public async Task<IActionResult> Index(string? category)
        {
            try
            {
                List<Article> articles = await Article.GetAllArticlesAsync(articleDAL);
                List<Category> categories = await Category.GetCategoriesAsync(categoryDAL);

                if (!string.IsNullOrEmpty(category))
                {
                    List<Article> filtered = new List<Article>();
                    foreach (Article article in articles)
                    {
                        if (article.Category.Name == category)
                            filtered.Add(article);
                    }
                    articles = filtered;
                }

                ViewBag.Categories = categories;
                return View(articles);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the catalog.";
                return RedirectToAction("Error");
            }
        }

        public IActionResult AddToCart(int _articleId, int _quantity = 1, string? category = null)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to add article to your shopping cart.";
                return RedirectToAction("SignUp");
            }

            Dictionary<int, int> cart = null;
            string jsonCart = HttpContext.Session.GetString("Cart");
            if(jsonCart != null)
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(jsonCart);
            else
                cart = new Dictionary<int, int>();

            if (cart.ContainsKey(_articleId))
                cart[_articleId] += _quantity;
            else
                cart[_articleId] = _quantity;

            HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            return RedirectToAction("Index", new { category = category });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int articleId, int quantity = 1)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to add article to your shopping cart.";
                return RedirectToAction("SignUp");
            }
            Dictionary<int, int> cart = null;
            string jsonCart = HttpContext.Session.GetString("Cart");
            if (jsonCart != null)
                cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(jsonCart);
            else
                cart = new Dictionary<int, int>();

            if (cart.ContainsKey(articleId))
                cart[articleId] += quantity;
            else
                cart[articleId] = quantity;

            HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveOneFromCart(int articleId)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to remove article from your shopping cart.";
                return RedirectToAction("SignUp");
            }
            string jsonCart = HttpContext.Session.GetString("Cart");
            if (jsonCart == null)
                return RedirectToAction("ShoppingCart");

            Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(jsonCart);

            if (cart.ContainsKey(articleId))
            {
                cart[articleId]--;
                if (cart[articleId] <= 0)
                    cart.Remove(articleId);
            }

            HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int articleId)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                TempData["Error"] = "You must be logged in to remove article from your shopping cart.";
                return RedirectToAction("SignUp");
            }
            string jsonCart = HttpContext.Session.GetString("Cart");
            if (jsonCart == null)
                return RedirectToAction("ShoppingCart");

            Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(jsonCart);

            cart.Remove(articleId);

            HttpContext.Session.SetString("Cart", System.Text.Json.JsonSerializer.Serialize(cart));
            return RedirectToAction("ShoppingCart");
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(user);
                }
                //verify if user is in DB
                if (await Client.GetClientByEmail(clientDAL, user.Email) != null)
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    return View(user);
                }
                // add user in DB and add to session
                int clientId = await user.AddClientAsync(clientDAL, user);
                if (clientId == -1)
                {
                    ModelState.AddModelError("", "An error occurred while creating your account.");
                    return View(user);
                }
                HttpContext.Session.SetInt32("Id", clientId);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("Type", "Client");

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Signing up.";
                return RedirectToAction("Error");
            }
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

            int cashierID = HttpContext.Session.GetInt32("CashierId").Value;
            string firstName = HttpContext.Session.GetString("FirstName");
            int storeId = HttpContext.Session.GetInt32("storeId").Value;

            try
            {
                Cashier currentCashier = await Cashier.GetCashierAsync(cashierID, storeId, employeeDAL);
                currentCashier.FirstName = firstName;

                List<Order> todaysOrders = await currentCashier.GetTodaysOrdersAsync(storeDAL);

                if (todaysOrders == null || todaysOrders.Count == 0)
                {
                    ViewBag.InfoMessage = "No orders to be fulfilled today.";
                }

                return View(todaysOrders);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "[ERROR]: " + ex.Message;
                return View(new List<Order>());
            }
        }
        public async Task<IActionResult> IndexPreparator()
        {
            if (HttpContext.Session.GetString("Type") != "Preparator" ||
                HttpContext.Session.GetInt32("Id") == null ||
                HttpContext.Session.GetInt32("storeId") == null)
            {
                return RedirectToAction("Connexion");
            }

            int preparatorID = HttpContext.Session.GetInt32("Id").Value;
            string firstName = HttpContext.Session.GetString("FirstName");

            int storeId = HttpContext.Session.GetInt32("storeId").Value;

            try
            {
                Preparator currentPreparator = await Preparator.GetPreparatorAsync(preparatorID, storeId, employeeDAL);
                currentPreparator.FirstName = firstName;

                List<Order> ordersToPrepare = await currentPreparator.GetOrderToPrepareAsync(storeDAL);

                if (ordersToPrepare == null || ordersToPrepare.Count == 0)
                {
                    ViewBag.InfoMessage = "No orders to be fulfilled today.";
                }

                return View(ordersToPrepare);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "[ERROR]: " + ex.Message;
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                if (HttpContext.Session.GetInt32("Id") == null)
                {
                    TempData["Error"] = "You must be logged in to view your profile.";
                    return RedirectToAction("SignUp");
                }
                string email = HttpContext.Session.GetString("Email");
                Client user = await Client.GetClientByEmail(clientDAL, email);
                return View(user);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Getting the client info.";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> EditProfile()
        {
            try
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
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Getting the client info.";
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Client user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(user);
                }
                //modification of user in DB
                int rows = await clientDAL.UpdateClientInfo(user);

                if (rows == 0)
                {
                    TempData["Error"] = "An error occurred while saving the changes";
                    return RedirectToAction("Error");
                }

                HttpContext.Session.SetString("FirstName", user.FirstName);
                return RedirectToAction("Profile");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Updating the client info.";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> ShoppingCart()
        {
            try
            {
                if (HttpContext.Session.GetInt32("Id") == null)
                {
                    TempData["Error"] = "You must be logged in to view your shopping cart.";
                    return RedirectToAction("SignUp");
                }

                string json = HttpContext.Session.GetString("Cart");
                if (json == null)
                    return View((Order)null);

                Dictionary<int, int> cartDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json);
                if (cartDict.Count == 0)
                    return View((Order)null);

                List<Article> articles = await articleDAL.GetArticlesAsync(cartDict.Keys.ToList());

                Order cart = new Order();
                foreach (Article article in articles)
                {
                    cart.AddArticle(article, cartDict[article.IDArticle]);
                }

                return View(cart);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Getting the Articles.";
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Checkout(int? storeId = null, string? selectedDate = null, string? timeSlotInfo = null)
        {
            try
            {
                if (HttpContext.Session.GetInt32("Id") == null)
                {
                    TempData["Error"] = "You must be logged in to checkout.";
                    return RedirectToAction("SignUp");
                }

                string json = HttpContext.Session.GetString("Cart");
                if (json == null)
                    return RedirectToAction("ShoppingCart");

                Dictionary<int, int> cartDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json);
                List<Article> articles = await articleDAL.GetArticlesAsync(cartDict.Keys.ToList());
                Order cart = new Order();
                foreach (Article article in articles)
                    cart.AddArticle(article, cartDict[article.IDArticle]);

                List<Store> stores = await Store.GetStoresAsync(storeDAL);
                ViewBag.Stores = stores;
                ViewBag.SelectedStoreId = storeId;

                if (storeId != null)
                {
                    Store selectedStore = new Store { Id = storeId.Value };
                    List<TimeSlot> timeSlots = await Store.GetAvailableTimeSlotsAsync(storeDAL, selectedStore);
                    ViewBag.TimeSlots = timeSlots;

                    List<DateTime> availableDates = new List<DateTime>();
                    foreach (TimeSlot slot in timeSlots)
                    {
                        if (!availableDates.Contains(slot.Date.Date))
                            availableDates.Add(slot.Date.Date);
                    }
                    ViewBag.AvailableDates = availableDates;
                }

                ViewBag.SelectedTimeSlotInfo = timeSlotInfo;
                ViewBag.SelectedDate = selectedDate;
                return View(cart);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Getting the Stores and/or time slots.";
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int storeId, string timeSlotInfo, string selectedDate)
        {
            try
            {
                if (HttpContext.Session.GetInt32("Id") == null)
                {
                    TempData["Error"] = "You must be logged in to place an order.";
                    return RedirectToAction("SignUp");
                }

                string json = HttpContext.Session.GetString("Cart");
                if (json == null)
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("ShoppingCart");
                }

                Dictionary<int, int> cart = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json);
                if (cart.Count == 0)
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("ShoppingCart");
                }

                string[] parts = timeSlotInfo.Split('|');
                int timeSlotId = int.Parse(parts[0]);
                DateTime date = DateTime.Parse(selectedDate);
                DateTime start = DateTime.Parse(selectedDate + " " + parts[1]);
                DateTime end = DateTime.Parse(selectedDate + " " + parts[2]);

                Client client = new Client { Id = HttpContext.Session.GetInt32("Id").Value };
                Store store = new Store { Id = storeId };
                TimeSlot timeSlot = new TimeSlot { Id = timeSlotId, Date = date, StartingHour = start, EndingHour = end };

                int orderId = await Order.PlaceOrderAsync(orderDAL, client, store, timeSlot, cart);

                if (orderId <= 0)
                {
                    TempData["Error"] = "An error occurred while placing your order.";
                    return RedirectToAction("Checkout");
                }

                HttpContext.Session.Remove("Cart");
                TempData["Success"] = "Your order has been placed successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while Inserting the order.";
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FinalizeOrder(int id)
        {

            Order currentOrder = await Order.GetOrderAsync(id, storeDAL);

            if (currentOrder == null)
            {
                ViewBag.ErrorMessage = "Commande introuvable.";
                return RedirectToAction("IndexCashier");
            }

            HttpContext.Session.SetInt32("CurrentOrderId", id);

            return View(currentOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelFinalizeOrder()
        {
            HttpContext.Session.Remove("CurrentOrderId");
            return RedirectToAction("IndexCashier"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItemFromOrder(int itemId)
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId == null) return RedirectToAction("IndexCashier");

            Order currentOrder = await Order.GetOrderAsync(orderId.Value, storeDAL);

            currentOrder.RemoveItem(itemId);

            decimal newPrice = currentOrder.GetTotalPrice();

            return View("FinalizeOrder"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageBoxReturn(int returnedBoxes)
        {
            int? orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId == null) return RedirectToAction("IndexCashier");

            Order currentOrder = await Order.GetOrderAsync(orderId.Value, storeDAL);

            currentOrder.ManageBoxReturn(returnedBoxes);

            decimal newPrice = currentOrder.GetTotalPrice();

            return View("FinalizeOrder");
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
