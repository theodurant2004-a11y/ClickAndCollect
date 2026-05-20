using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;

namespace ClickAndCollect.DAL
{
    public class OrderDAL : IOrderDAL
    {
        private readonly string connectionString;

        public OrderDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public async Task<int> PlaceOrderAsync(Client client, Store store, TimeSlot timeSlot, Dictionary<int, int> cart)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    @"DECLARE @TimeSlotID INT = @ExistingTimeSlotId;
              IF @TimeSlotID = 0
              BEGIN
                  INSERT INTO Timeslot (date_, startingHour, endHour, storeID)
                  VALUES (@Date, @StartingHour, @EndingHour, @StoreId);
                  SET @TimeSlotID = SCOPE_IDENTITY();
              END
              INSERT INTO Order_ (boxUsed, boxReturned, serviceCharge, status, storeID, timeSlotID, personID)
              VALUES (NULL, NULL, @ServiceCharge, 'Ordered', @StoreId, @TimeSlotID, @PersonId);
              SELECT SCOPE_IDENTITY();", con);

                cmd.Parameters.AddWithValue("@ExistingTimeSlotId", timeSlot.Id);
                cmd.Parameters.AddWithValue("@Date", timeSlot.Date.Date);
                cmd.Parameters.AddWithValue("@StartingHour", timeSlot.StartingHour.TimeOfDay);
                cmd.Parameters.AddWithValue("@EndingHour", timeSlot.EndingHour.TimeOfDay);
                cmd.Parameters.AddWithValue("@StoreId", store.Id);
                cmd.Parameters.AddWithValue("@PersonId", client.Id);
                cmd.Parameters.AddWithValue("@ServiceCharge", 5.95m);

                int orderId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                foreach (KeyValuePair<int, int> line in cart)
                {
                    SqlCommand cmdLine = new SqlCommand(
                        "INSERT INTO OrderLine (articleID, orderID, quantity) VALUES (@ArticleId, @OrderId, @Quantity)", con);
                    cmdLine.Parameters.AddWithValue("@ArticleId", line.Key);
                    cmdLine.Parameters.AddWithValue("@OrderId", orderId);
                    cmdLine.Parameters.AddWithValue("@Quantity", line.Value);
                    await cmdLine.ExecuteNonQueryAsync();
                }

                return orderId;
            }
        }

        public async Task<int> FinalizeOrderAsync(Order order)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Order_
                      SET status      = @Status,
                          boxUsed     = @BoxUsed,
                          boxReturned = @BoxReturned
                      WHERE orderID   = @OrderId", con);

                cmd.Parameters.AddWithValue("@Status", order.Status);
                cmd.Parameters.AddWithValue("@BoxUsed", order.BoxUsed);
                cmd.Parameters.AddWithValue("@BoxReturned", order.BoxReturned);
                cmd.Parameters.AddWithValue("@OrderId", order.OrderID);

                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            Order order = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // 1. Charger la commande + client + timeslot
                SqlCommand cmdOrder = new SqlCommand(
                    @"SELECT o.orderID,
                             o.boxUsed,
                             o.boxReturned,
                             o.serviceCharge,
                             o.status,
                             o.personID,
                             p.firstName,
                             p.lastname,
                             t.date_,
                             t.startingHour,
                             t.endHour
                      FROM Order_ o
                      JOIN Client c   ON o.personID   = c.personID
                      JOIN Person p   ON c.personID   = p.personID
                      JOIN Timeslot t ON o.timeSlotID = t.timeSlotID
                      WHERE o.orderID = @OrderId", con);

                cmdOrder.Parameters.AddWithValue("@OrderId", orderId);

                using (SqlDataReader reader = await cmdOrder.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int boxUsed = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        int boxReturned = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        decimal serviceCharge = reader.GetDecimal(3);
                        string status = reader.GetString(4);
                        int personID = reader.GetInt32(5);
                        string firstName = reader.GetString(6);
                        string lastName = reader.GetString(7);
                        DateTime date = reader.GetDateTime(8);
                        TimeSpan startSp = reader.GetTimeSpan(9);
                        TimeSpan endSp = reader.GetTimeSpan(10);

                        Client tempClient = new Client { Id = personID, FirstName = firstName, SurName = lastName };
                        TimeSlot tempTimeSlot = new TimeSlot(date, date.Add(startSp), date.Add(endSp));

                        order = new Order
                        {
                            OrderID = orderId,
                            BoxUsed = boxUsed,
                            BoxReturned = boxReturned,
                            ServiceCharge = serviceCharge,
                            Status = status,
                            Client = tempClient,
                            TimeSlot = tempTimeSlot
                        };
                    }
                }

                if (order == null)
                    return null;

                // 2. Charger les OrderLines + Articles
                SqlCommand cmdLines = new SqlCommand(
                    @"SELECT ol.quantity,
                             a.articleID,
                             a.name,
                             a.price,
                             a.description,
                             COALESCE(a.imagePath, 'none') AS imagePath,
                             c.name AS categoryName
                      FROM OrderLine ol
                      JOIN Article a  ON ol.articleID  = a.articleID
                      JOIN Category c ON a.categoryID  = c.categoryID
                      WHERE ol.orderID = @OrderId", con);

                cmdLines.Parameters.AddWithValue("@OrderId", orderId);

                using (SqlDataReader reader = await cmdLines.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int quantity = reader.GetInt32(0);
                        int articleId = reader.GetInt32(1);
                        string name = reader.GetString(2);
                        decimal price = reader.GetDecimal(3);
                        string desc = reader.GetString(4);
                        string imagePath = reader.GetString(5);
                        string catName = reader.GetString(6);

                        Article article = new Article(articleId, name, price, desc, new Category(catName), imagePath);
                        order.AddArticle(article, quantity);
                    }
                }
            }

            return order;
        }
        public async Task<int> RemoveItemFromOrderAsync(int orderId, int articleId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Decrease quantity by 1
                SqlCommand cmdCheck = new SqlCommand(
                    "SELECT quantity FROM OrderLine WHERE orderID = @OrderId AND articleID = @ArticleId", con);
                cmdCheck.Parameters.AddWithValue("@OrderId", orderId);
                cmdCheck.Parameters.AddWithValue("@ArticleId", articleId);
                int qty = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());

                if (qty <= 1)
                {
                    // Remove the line entirely
                    SqlCommand cmdDel = new SqlCommand(
                        "DELETE FROM OrderLine WHERE orderID = @OrderId AND articleID = @ArticleId", con);
                    cmdDel.Parameters.AddWithValue("@OrderId", orderId);
                    cmdDel.Parameters.AddWithValue("@ArticleId", articleId);
                    return await cmdDel.ExecuteNonQueryAsync();
                }
                else
                {
                    SqlCommand cmdUpd = new SqlCommand(
                        "UPDATE OrderLine SET quantity = quantity - 1 WHERE orderID = @OrderId AND articleID = @ArticleId", con);
                    cmdUpd.Parameters.AddWithValue("@OrderId", orderId);
                    cmdUpd.Parameters.AddWithValue("@ArticleId", articleId);
                    return await cmdUpd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<int> PrepareOrderAsync(Order order)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Order_
                      SET status  = @Status,
                          boxUsed = @BoxUsed
                      WHERE orderID = @OrderId", con);

                cmd.Parameters.AddWithValue("@Status", order.Status);
                cmd.Parameters.AddWithValue("@BoxUsed", order.BoxUsed);
                cmd.Parameters.AddWithValue("@OrderId", order.OrderID);

                return await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}