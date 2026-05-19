using ClickAndCollect.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            List<Order> orders = new List<Order>();
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
                        decimal serviceCharge = reader.GetDecimal(3);
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
                        orders.Add(order);
                    }
                }
            }
            return orders;
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
                                ServiceCharge = reader.GetDecimal(3),
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

        public async Task<List<Store>> GetStoresAsync()
        {
            List<Store> stores = new List<Store>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT s.storeID, s.name, a.roadName, a.roadNumber, pc.postalCode, pc.cityName
              FROM Store s
              JOIN Address a ON s.addressID = a.addressID
              JOIN PostalCode pc ON a.postalCodeID = pc.postalCodeID", con);

                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        stores.Add(new Store
                        {
                            Id = reader.GetInt32("storeID"),
                            Name = reader.GetString("name"),
                            RoadName = reader.GetString("roadName"),
                            RoadNumber = reader.GetString("roadNumber"),
                            PostalCode = reader.GetInt32("postalCode").ToString(),
                            City = reader.GetString("cityName")
                        });
                    }
                }
            }
            return stores;
        }

        public async Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(Store _store)
        {
            List<TimeSlot> existingSlots = new List<TimeSlot>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.timeSlotID, t.date_, t.startingHour, t.endHour, COUNT(o.orderID) AS reservationCount
              FROM Timeslot t
              LEFT JOIN Order_ o ON t.timeSlotID = o.timeSlotID
              WHERE t.storeID = @StoreId
              AND t.date_ > CAST(GETDATE() AS DATE)
              GROUP BY t.timeSlotID, t.date_, t.startingHour, t.endHour", con);

                cmd.Parameters.AddWithValue("@StoreId", _store.Id);

                await con.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        DateTime date = reader.GetDateTime("date_");
                        TimeSpan startSpan = reader.GetTimeSpan(2);
                        TimeSpan endSpan = reader.GetTimeSpan(3);
                        int reservationCount = reader.GetInt32("reservationCount");
                        existingSlots.Add(new TimeSlot
                        {
                            Id = reader.GetInt32("timeSlotID"),
                            Date = date,
                            StartingHour = date.Add(startSpan),
                            EndingHour = date.Add(endSpan),
                            IsFull = reservationCount >= 10
                        });
                    }
                }
            }

            List<TimeSlot> availableSlots = new List<TimeSlot>();
            for (int day = 1; day <= 7; day++)
            {
                DateTime date = DateTime.Today.AddDays(day);
                for (int hour = 8; hour < 18; hour++)
                {
                    DateTime start = date.AddHours(hour);
                    DateTime end = date.AddHours(hour + 1);

                    TimeSlot existing = null;
                    foreach (TimeSlot slot in existingSlots)
                    {
                        if (slot.Date.Date == date.Date && slot.StartingHour.Hour == hour)
                        {
                            existing = slot;
                            break;
                        }
                    }

                    if (existing != null)
                        availableSlots.Add(existing);
                    else
                        availableSlots.Add(new TimeSlot { Date = date, StartingHour = start, EndingHour = end, IsFull = false });
                }
            }

            return availableSlots;
        }
    }
}
