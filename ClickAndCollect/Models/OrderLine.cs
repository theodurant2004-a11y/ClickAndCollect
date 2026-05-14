namespace ClickAndCollect.Models
{
    public class OrderLine
    {
		private int quantity;
		private Article article;
		private Order parentOrder;

		public Order ParentOrder
		{
			get { return parentOrder; }
			set 
			{
				ArgumentNullException.ThrowIfNull(value);
                parentOrder = value; 
			}
		}

		
		public Article Article_
		{
			get { return article; }
			set 
			{
				ArgumentNullException.ThrowIfNull(value);
                article = value; 
			}
		}


		public int Quantity
		{
			get { return quantity; }
			set 
			{
				if(value <= 0)
					ParentOrder.RemoveOrderLine(this);
                quantity = value;
			}
		}

        public OrderLine()
        {
        }

        public OrderLine(int _quantity, Article _article, Order _parentOrder)
        {
            Quantity = _quantity;
            Article_ = _article;
            ParentOrder = _parentOrder;
        }

        public decimal GetPriceLine()
		{
			return Quantity * Article_.Price;
        }
	}
}
