using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

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

        public User GetByUserId(string userId)
        {
            return this.GetDbSet<User>().FirstOrDefault(x => x.Username == userId);
        }

        public void AddUser(User user)
        {
            this.GetDbSet<User>().Add(user);
            this.UnitOfWork.SaveChanges();
        }

    }
}
