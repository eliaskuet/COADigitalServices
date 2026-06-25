using System.Threading.Tasks;

namespace COADigitalServices.BLL.Services
{
    public interface IUserService
    {
        Task<bool> ValidateUserAsync(string username, string password);
    }
}
