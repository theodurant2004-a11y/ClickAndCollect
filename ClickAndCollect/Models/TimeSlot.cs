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
                    throw new ArgumentException("ID cannot be negative or zero.");
                id = value;
            }
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (value < DateTime.Today)
                    throw new ArgumentException("Date cannot be in the past.");
                date = value;
            }
        }

        public DateTime StartingHour
        {
            get { return startingHour; }
            set
            {
                if (value < DateTime.Today)
                    throw new ArgumentException("Starting hour cannot be in the past.");
                startingHour = value;
            }
        }

        public DateTime EndingHour
        {
            get { return endingHour; }
            set
            {
                if (value < DateTime.Today)
                    throw new ArgumentException("Ending hour cannot be in the past.");
                endingHour = value;
            }
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

        public override string ToString()
        {
            return $"{Date:dd/MM/yyyy} {StartingHour:HH:mm} - {EndingHour:HH:mm}";
        }
    }
}