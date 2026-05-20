using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace ClickAndCollect.DAL
{
    public class StoreDAL : IStoreDAL
    {
        private readonly string connectionString;

        public StoreDAL(string _connectionString)
        {
            connectionString = _connectionString;
        }

        public async Task<List<Order>> GetTodaysOrdersAsync(Cashier cashier)
        {
            List<Order> allStoreOrders = new List<Order>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT o.orderID, 
                            o.boxUsed, 
                            o.boxReturned, 
                            o.serviceCharge, 
                            o.status, 
                            o.storeID, 
                            o.timeSlotID, 
                            o.personID, 
                            p.firstName, 
                            p.lastname, 
                            t.startingHour, 
                            t.endHour, 
                            t.date_
                        FROM Order_ o
                        JOIN Timeslot t ON o.timeSlotID = t.timeSlotID
                        JOIN Client c   ON o.personID = c.personID
                        JOIN Person p   ON c.personID = p.personID"
                        , con
                    );

                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int orderId = reader.GetInt32(0);
                        int boxUsed = reader.GetInt32(1);
                        int boxReturned = reader.GetInt32(2);
                        double serviceCharge = (double)reader.GetDecimal(3);
                        string status = reader.GetString(4);
                        int storeID = reader.GetInt32(5);
                        int timeSlotID = reader.GetInt32(6);
                        int personID = reader.GetInt32(7);
                        string firstName = reader.GetString(8);
                        string lastName = reader.GetString(9);
                        TimeSpan startingHourSpan = reader.GetTimeSpan(10);
                        TimeSpan endingHourSpan = reader.GetTimeSpan(11);
                        DateTime date = reader.GetDateTime(12);

                        DateTime startingHour = date.Add(startingHourSpan);
                        DateTime endingHour = date.Add(endingHourSpan);

                        int currentId = reader.GetInt32(5);

                        if (currentId == cashier.StoreID && date.Date >= DateTime.Today)
                        {
                            Client tempClient = new Client { Id = personID, FirstName = firstName, SurName = lastName };
                            TimeSlot tempTimeSlot = new TimeSlot(date, startingHour, endingHour);
                            Order order = new Order
                            {
                                OrderID = orderId,
                                BoxUsed = boxUsed,
                                BoxReturned = boxReturned,
                                ServiceCharge = serviceCharge,
                                Status = status,
                                Client = tempClient,
                                TimeSlot = tempTimeSlot
                            };
                            allStoreOrders.Add(order);
                        }
                    }
                }
            }

            List<Order> todaysOrders = new List<Order>();

            foreach (var order in allStoreOrders)
            {
                if (order.Status == "Prepared" && order.TimeSlot.Date.Date == DateTime.Today)
                {
                    todaysOrders.Add(order);
                }
            }
            return todaysOrders;
        }

        public async Task<List<Order>> GetOrderToPrepareAsync(Preparator preparator)
        {
            List<Order> allStoreOrders = new List<Order>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand
                    (
                        @"SELECT 
                            o.orderID, 
                            o.boxUsed, 
                            o.boxReturned, 
                            o.serviceCharge, 
                            o.status, 
                            o.storeID, 
                            o.timeSlotID, 
                            o.personID, 
                            p.firstName, 
                            p.lastname, 
                            t.startingHour, 
                            t.endHour, 
                            t.date_
                        FROM Order_ o
                        JOIN Timeslot t ON o.timeSlotID = t.timeSlotID
                        JOIN Client c   ON o.personID = c.personID
                        JOIN Person p   ON c.personID = p.personID", con
                    );

                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int currentId = reader.GetInt32(5);
                        DateTime date = reader.GetDateTime(12);

                        if (currentId == preparator.StoreID && date.Date >= DateTime.Today)
                        {
                            TimeSpan startingHourSpan = reader.GetTimeSpan(10);
                            TimeSpan endingHourSpan = reader.GetTimeSpan(11);
                            DateTime startingHour = date.Add(startingHourSpan);
                            DateTime endingHour = date.Add(endingHourSpan);

                            Client tempClient = new Client { Id = reader.GetInt32(7), FirstName = reader.GetString(8), SurName = reader.GetString(9) };
                            TimeSlot tempTimeSlot = new TimeSlot(date, startingHour, endingHour);

                            Order order = new Order
                            {
                                OrderID = reader.GetInt32(0),
                                BoxUsed = reader.GetInt32(1),
                                BoxReturned = reader.GetInt32(2),
                                ServiceCharge = (double)reader.GetDecimal(3),
                                Status = reader.GetString(4),
                                Client = tempClient,
                                TimeSlot = tempTimeSlot
                            };
                            allStoreOrders.Add(order);
                        }
                    }
                }
            }

            List<Order> ordersToPrepare = new List<Order>();
            foreach (var order in allStoreOrders)
            {
                if (order.Status == "Ordered" && order.TimeSlot.Date.Date == DateTime.Today)
                {
                    ordersToPrepare.Add(order);
                }
            }
            return ordersToPrepare;
        }
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            Order currentOrder = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmdOrder = new SqlCommand(
                    @"SELECT o.orderID, o.boxUsed, o.boxReturned, o.serviceCharge, o.status, 
                             o.storeID, o.timeSlotID, o.personID, p.firstName, p.lastname
                      FROM Order_ o
                      JOIN Client c ON o.personID = c.personID
                      JOIN Person p ON c.personID = p.personID
                      WHERE o.orderID = @orderId", con);

                cmdOrder.Parameters.AddWithValue("@orderId", orderId);

                await con.OpenAsync();

                using (SqlDataReader reader = await cmdOrder.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        Client tempClient = new Client { Id = reader.GetInt32(7), FirstName = reader.GetString(8), SurName = reader.GetString(9) };

                        currentOrder = new Order
                        {
                            OrderID = reader.GetInt32(0),
                            BoxUsed = reader.GetInt32(1),
                            BoxReturned = reader.GetInt32(2),
                            ServiceCharge = (double)reader.GetDecimal(3),
                            Status = reader.GetString(4),
                            Client = tempClient
                        };
                    }
                }

                if (currentOrder != null)
                {
                    SqlCommand cmdLines = new SqlCommand(
                        @"SELECT ol.quantity, a.articleID, a.name, a.price
                          FROM OrderLine ol
                          JOIN Article a ON ol.articleID = a.articleID
                          WHERE ol.orderID = @orderId", con);

                    cmdLines.Parameters.AddWithValue("@orderId", orderId);

                    using (SqlDataReader readerLines = await cmdLines.ExecuteReaderAsync())
                    {
                        while (await readerLines.ReadAsync())
                        {
                            int quantity = readerLines.GetInt32(0);

                            Article tempArticle = new Article
                            {
                                IDArticle = readerLines.GetInt32(1),
                                NameProduct = readerLines.GetString(2),
                                Price = readerLines.GetDecimal(3)
                            };

                            currentOrder.AddArticle(tempArticle, quantity);
                        }
                    }
                }
            }

            return currentOrder;
        }
    }
}
