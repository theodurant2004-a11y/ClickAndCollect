namespace ClickAndCollect.Models
{
    public class TimeSlot
    {
        private DateTime date;
        private DateTime startingHour;
        private DateTime endingHour;
        private int id;

        public bool IsFull { get; set; } = false;

        public int Id
        {
            get { return id; }
            init
            {
                if (value < 0)
                    throw new ArgumentException("ID cannot be negative.");
                id = value;
            }
        }

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        public DateTime StartingHour
        {
            get { return startingHour; }
            set { startingHour = value; }
        }

        public DateTime EndingHour
        {
            get { return endingHour; }
            set { endingHour = value; }
        }

        public TimeSlot()
        {
        }

        public TimeSlot(DateTime _date, DateTime _startingHour, DateTime _endingHour)
        {
            Date = _date;
            StartingHour = _startingHour;
            EndingHour = _endingHour;
        }

        public bool IsValidForBooking()
        {
            return Date.Date >= DateTime.Today
                && StartingHour >= DateTime.Now
                && EndingHour > StartingHour;
        }

        public override string ToString()
        {
            return $"{Date:dd/MM/yyyy} {StartingHour:HH:mm} - {EndingHour:HH:mm}";
        }
    }
}