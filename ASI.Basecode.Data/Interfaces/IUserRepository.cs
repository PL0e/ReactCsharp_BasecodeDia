using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> GetUsers();
        User GetEmail(string email);
        IEnumerable<User> GetAll();
        User GetById(int id);
        Task AddAsync(User user);
        Task DeleteAsync(User user);
        Task UpdateAsync(User user);

    }
}
