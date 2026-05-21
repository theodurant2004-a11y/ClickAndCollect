using ClickAndCollect.DAL;

namespace ClickAndCollect.Models
{
    public class Order
    {
        public Client Client { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public Store Store { get; set; }

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
            OrderLine existingLine = null;
            for (int i = 0; i < orderLines.Count && existingLine == null; i++)
            {
                if (orderLines[i].Article_ == _article)
                    existingLine = orderLines[i];
            }
            if (existingLine != null)
                existingLine.Quantity += _quantity;
            else
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
            orderLines.Remove(_line);
        }

        public decimal GetArticlesTotal()
        {
            decimal total = 0;
            foreach (OrderLine line in orderLines)
                total += line.GetPriceLine();
            return total;
        }

        public decimal GetTotalPrice()
        {
            decimal boxDeposit = BoxUsed * 5.95m;
            decimal boxRefund = BoxReturned * 5.95m;
            return GetArticlesTotal() + ServiceCharge + boxDeposit - boxRefund;
        }

        public void FlushOrder()
        {
            orderLines.Clear();
        }

        public static async Task<int> PlaceOrderAsync(IOrderDAL _orderDAL, Client _client, Store _store, TimeSlot _timeSlot, Dictionary<int, int> _cart)
        {
            return await _orderDAL.PlaceOrderAsync(_client, _store, _timeSlot, _cart);
        }

        public static async Task<Order> GetOrderByIdAsync(IOrderDAL _orderDAL, int _orderId)
        {
            return await _orderDAL.GetOrderByIdAsync(_orderId);
        }

        public async Task<int> FinalizeOrderAsync(IOrderDAL _orderDAL, int _boxUsed, int _boxReturned)
        {
            if (_boxUsed < 0)
                throw new ArgumentException("BoxUsed cannot be negative.");
            if (_boxReturned < 0)
                throw new ArgumentException("BoxReturned cannot be negative.");

            BoxUsed = _boxUsed;
            BoxReturned = _boxReturned;
            Status = "Delivered";

            return await _orderDAL.FinalizeOrderAsync(this);
        }

        public async Task<int> UpdateBoxesAsync(IOrderDAL _orderDAL, int _boxUsed, int _boxReturned)
        {
            if (_boxUsed < 0)
                throw new ArgumentException("BoxUsed cannot be negative.");
            if (_boxReturned < 0)
                throw new ArgumentException("BoxReturned cannot be negative.");

            BoxUsed = _boxUsed;
            BoxReturned = _boxReturned;

            return await _orderDAL.FinalizeOrderAsync(this);
        }

        public async Task<int> ChangeStatusAsync(IOrderDAL _orderDAL, string _status)
        {
            if (string.IsNullOrWhiteSpace(_status))
                throw new ArgumentException("Status cannot be empty.");

            Status = _status;

            return await _orderDAL.FinalizeOrderAsync(this);
        }

        public async Task<int> PrepareOrderAsync(IOrderDAL _orderDAL, int _boxUsed, string _status)
        {
            if (_boxUsed < 0)
                throw new ArgumentException("BoxUsed cannot be negative.");
            if (_status != "InPreparation" && _status != "Prepared" && _status != "Ordered")
                throw new ArgumentException("Invalid status for preparation.");

            BoxUsed = _boxUsed;
            Status = _status;

            return await _orderDAL.PrepareOrderAsync(this);
        }
    }
}