using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers()
                              .Where(x => x.Username == userId && x.Password == passwordKey)
                              .FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public bool UserExists(string userId)
        {
            return _repository.GetByUserId(userId) != null;
        }

        public void RegisterUser(string userId, string name, string password)
        {
            var role = userId.StartsWith("30") ? "chairman"
                     : userId.StartsWith("10") ? "adviser"
                     : "student";

            var newUser = new User
            {
                Username = userId,
                Password = PasswordManager.EncryptPassword(password),
                Role = role,
            };

            _repository.AddUser(newUser);
        }
    }
}
