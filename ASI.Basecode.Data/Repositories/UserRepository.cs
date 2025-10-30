using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public User GetEmail(string email)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await this.GetDbSet<User>().AddAsync(user);
            await UnitOfWork.SaveChangesAsync(); 
        }

        public IEnumerable<User> GetAll()
        {
            return this.GetDbSet<User>().ToList();
        }

        public User GetById(int id)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.Id == id);
        }

        public async Task DeleteAsync(User user)
        {
            this.GetDbSet<User>().Remove(user);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            this.GetDbSet<User>().Update(user);
            await UnitOfWork.SaveChangesAsync();
        }

    }
}
