using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    public abstract class User
    {
        private int id;
        private string? firstName;
        private string? surName;
        private string? email;
        private string? password;
        private string? confirmPassword;

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword
        {
            get { return confirmPassword; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                confirmPassword = value;
            }
        }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{5,32}$",
            ErrorMessage = "Password must be between 5 and 32 characters and contain at least one uppercase letter, one lowercase letter and one number.")]
        public string? Password
        {
            get { return password; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Length < 5)
                    throw new ArgumentException("Password must be at least 5 characters long.");
                if (value.Length > 32)
                    throw new ArgumentException("Password cannot be longer than 32 characters.");
                password = value;
            }
        }

        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? Email
        {
            get { return email; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                email = value;
            }
        }

        [Display(Name = "Surname")]
        [MinLength(2, ErrorMessage = "Surname must be at least 2 characters long.")]
        [MaxLength(25, ErrorMessage = "Surname cannot be longer than 25 characters.")]
        public string? SurName
        {
            get { return surName; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Length < 2)
                    throw new ArgumentException("Surname must be at least 2 characters long.");
                if (value.Length > 25)
                    throw new ArgumentException("Surname cannot be longer than 25 characters.");
                surName = value;
            }
        }

        [Display(Name = "First Name")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        [MaxLength(25, ErrorMessage = "First name cannot be longer than 25 characters.")]
        public string? FirstName
        {
            get { return firstName; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Length < 2)
                    throw new ArgumentException("First name must be at least 2 characters long.");
                if (value.Length > 25)
                    throw new ArgumentException("First name cannot be longer than 25 characters.");
                firstName = value;
            }
        }

        public int Id
        {
            get { return id; }
            init
            {
                if (value <= 0)
                    throw new ArgumentException("ID cannot be negative or zero.");
                id = value;
            }
        }

        protected User()
        {
        }

        protected User(int _id, string _firstName, string _surName, string _email, string _password)
        {
            Id = _id;
            FirstName = _firstName;
            SurName = _surName;
            Email = _email;
            Password = _password;
        }
    }
}
