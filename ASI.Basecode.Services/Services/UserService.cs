using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public User GetEmail(string email)
        {
            return _repository.GetEmail(email);
        }

        public async Task AddAsync(User user)
        {
            await _repository.AddAsync(user);
        }

        public IEnumerable<User> GetUsers()
        {
            return _repository.GetAll(); 
        }


        public LoginResult AuthenticateUser(string email, string password, ref User user)
        {
            Console.WriteLine($"DEBUG: Attempting login for email: '{email}'");

            user = _repository.GetEmail(email);
            Console.WriteLine($"DEBUG: Found user? {(user != null ? "YES" : "NO")}");

            if (user == null)
                return LoginResult.Failed;

            Console.WriteLine($"DEBUG: DB Password: {user.Password}");
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);
            Console.WriteLine($"DEBUG: Password match result: {passwordMatch}");

            return passwordMatch ? LoginResult.Success : LoginResult.Failed;
        }



        public User GetById(int id)
        {
            return _repository.GetById(id);
        }

        public async Task DeleteAsync(User user)
        {
            await _repository.DeleteAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            var existingUser = _repository.GetById(user.Id);
            if (existingUser == null)
                throw new Exception("User not found.");

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = user.Password;
            }

            await _repository.UpdateAsync(existingUser);
        }




    }
}
