using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Order
    {
        public Client Client { get; set; }
        public TimeSlot TimeSlot { get; set; }

        private int orderID;
        private int boxUsed;
		    private int boxReturned;
        private string status;
        private decimal serviceCharge;
		    List<OrderLine> orderLines;

        public int OrderID
        {
            get { return orderID; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Order ID cannot be negative.");
                orderID = value;
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Status cannot be null or empty.");
                status = value;
            }
        }

        public decimal ServiceCharge
        {
            get { return serviceCharge; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Service charge cannot be negative.");
                serviceCharge = value;
            }
        }

        public int BoxUsed
        {
            get { return boxUsed; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Box used cannot be negative.");
                boxUsed = value;
            }
        }

        public int BoxReturned
        {
            get { return boxReturned; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Box returned cannot be negative.");
                boxReturned = value;
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Status cannot be null or empty.");
                status = value;
            }
        }

        public double ServiceCharge
        {
            get { return serviceCharge; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Service charge cannot be negative.");
                serviceCharge = value;
            }
        }

        public List<OrderLine> OrderLines
        {
            get { return orderLines; }
        }

        public Order()
        {
            orderLines = new List<OrderLine>();
        }

		public void AddArticle(Article _article, int _quantity)
		{
            //if article already exists in the order, update the quantity
            OrderLine existingLine = null;
            for (int i = 0; i < orderLines.Count && existingLine == null; i++)
            {
                if (orderLines[i].Article_ == _article)
                    existingLine = orderLines[i];
            }
			if(existingLine != null)
				existingLine.Quantity += _quantity;
            else
                //create a new order line with article and quantity
                orderLines.Add(new OrderLine(_quantity, _article, this));
        }

        public void RemoveArticle(Article _article, int _quantity)
        {
            for (int i = 0; i < orderLines.Count; i++)
            {
                if (orderLines[i].Article_ == _article)
                    orderLines[i].Quantity -= _quantity;
            }
        }

        public void RemoveOrderLine(OrderLine _line)
        {
            //TODO : Dispose of the orderline
            orderLines.Remove(_line);
        }

        public decimal GetTotalPrice()
        {
            decimal totalPrice = 0;
            foreach (OrderLine line in orderLines)
            {
                totalPrice += line.GetPriceLine();
            }
            return totalPrice;
        }

        public void FlushOrder()
        {
            orderLines.Clear();
        }

        public static async Task<int> PlaceOrderAsync(IOrderDAL _orderDAL, Client _client, Store _store, TimeSlot _timeSlot, Dictionary<int, int> _cart)
        {
            return await _orderDAL.PlaceOrderAsync(_client, _store, _timeSlot, _cart);
        }
    }
}
