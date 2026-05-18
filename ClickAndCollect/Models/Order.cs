namespace ClickAndCollect.Models
{
    public class Order
    {
		private int boxUsed;
		private int boxReturned;
		// status
		// service charge
		List<OrderLine> orderLines;

        public int BoxUsed
		{
			get { return boxUsed; }
			set 
			{
				if(value < 0)
					throw new ArgumentException("Box used cannot be negative.");
                boxUsed = value; 
			}
		}
		
		public int BoxReturned
        {
			get { return boxReturned; }
			set 
			{
				if(value < 0)
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

		public void AddArticle(Article _article, int _quantity)//possibilité de modifier avec un contains ou quoi
		{
            //if article already exists in the order, update the quantity
            OrderLine existingLine = null;
			for(int i = 0; i < orderLines.Count && existingLine == null; i++)
            {
                if (orderLines[i].Article_.IDArticle == _article.IDArticle)
                    existingLine = orderLines[i];
            }
			if(existingLine != null)
				existingLine.Quantity += _quantity;
            //else create a new order line with article and quantity
            else
                orderLines.Add(new OrderLine(_quantity, _article, this));
        }

		public void RemoveArticle(Article _article, int _quantity)
		{
            for (int i = 0; i < orderLines.Count; i++)
            {
                if (orderLines[i].Article_.IDArticle == _article.IDArticle)
                    orderLines[i].Quantity -= _quantity;
            }
        }

        public void RemoveOrderLine(OrderLine _line)
        {
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
            foreach (OrderLine line in orderLines)
            {
                orderLines.Clear();
            }
        }
    }
}
