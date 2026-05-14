using ClickAndCollect.DAL;
using System.ComponentModel.DataAnnotations;

namespace ClickAndCollect.Models
{
    public class Client : User
    {
        private string? phone;
        private string? address;

        [Display(Name = "Address")]
        public string? Address
        {
            get { return address; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                address = value;
            }
        }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? Phone
        {
            get { return phone; }
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                phone = value;
            }
        }

        public Client()
        { 
        }

        public Client(int _id, string _firstName, string _surName, string _email, string _password, string _phone, string _address)
            : base(_id, _firstName, _surName, _email, _password)
        {
            Phone = _phone;
            Address = _address;
        }

        public static async Task<Client> GetClientByEmail(IClientDAL _dAL, string _email)
        {
            return await _dAL.GetClientByEmail(_email);
        }

        public async Task<int> AddClientAsync(IClientDAL _dAL, Client _client)
        {
            return await _dAL.AddClientAsync(_client);
        }
    }
}
