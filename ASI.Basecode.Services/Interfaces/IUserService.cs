using ASI.Basecode.Data.Models;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        bool UserExists(string userId);
        void RegisterUser(string userId, string name, string password);
    }
}
