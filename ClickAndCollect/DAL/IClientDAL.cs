using ClickAndCollect.Models;

namespace ClickAndCollect.DAL
{
    public interface IClientDAL
    {
        Task<Client> GetClientByEmail(string _email);

        Task<int> AddClientAsync(Client client);
    }
}
