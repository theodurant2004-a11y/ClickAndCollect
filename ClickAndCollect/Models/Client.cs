using ClickAndCollect.DAL;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    public class Client : User
    {
        private string phone;
        private string roadName;
        private string roadNumber;
        private string city;
        private string postalCode;

        [Display(Name = "Road Name")]
        [Required(ErrorMessage = "Road name is required.")]
        public string RoadName
        {
            get { return roadName; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                roadName = value;
            }
        }

        [Display(Name = "Road Number")]
        [Required(ErrorMessage = "Road number is required.")]
        public string RoadNumber
        {
            get { return roadNumber; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                roadNumber = value;
            }
        }

        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required.")]
        public string City
        {
            get { return city; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                city = value;
            }
        }

        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "Postal code is required.")]
        public string PostalCode
        {
            get { return postalCode; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                postalCode = value;
            }
        }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [DataType(DataType.PhoneNumber)]
        public string Phone
        {
            get { return phone; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                phone = value;
            }
        }

        public Client() { }

        public Client(int _id, string _firstName, string _surName, string _email, string _password, string _phone, string _roadName, string _roadNumber, string _city, string _postalCode)
            : base(_id, _firstName, _surName, _email, _password)
        {
            Phone = _phone;
            RoadName = _roadName;
            RoadNumber = _roadNumber;
            City = _city;
            PostalCode = _postalCode;
        }

        public static async Task<Client> GetClientByEmail(IClientDAL _dAL, string _email)
        {
            ArgumentNullException.ThrowIfNull(_email);
            return await _dAL.GetClientByEmail(_email);
        }

        public async Task<int> AddClientAsync(IClientDAL _dAL, Client _client)
        {
            ArgumentNullException.ThrowIfNull(_client);
            return await _dAL.AddClientAsync(_client);
        }

        public async Task<int> UpdateClientInfo(IClientDAL _dAL, int? _id, Client _client)
        {
            ArgumentNullException.ThrowIfNull(_client);
            return await _dAL.UpdateClientInfo(_id, _client);
        }
    }
}
