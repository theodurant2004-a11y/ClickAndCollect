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
              VALUES (NULL, NULL, @ServiceCharge, 'Pending', @StoreId, @TimeSlotID, @PersonId);

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
    }
}