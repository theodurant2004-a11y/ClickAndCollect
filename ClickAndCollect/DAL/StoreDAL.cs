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
    }
}
