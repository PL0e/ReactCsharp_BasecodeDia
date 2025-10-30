using System;
using System.Collections.Generic;
using ASI.Basecode.Data.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback> GetByIdAsync(int id);
        Task AddAsync(Feedback feedback);
        Task DeleteAsync(int id);
        Task<double> GetAverageRatingAsync();
    }
}
