using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class FeedbackRepository : BaseRepository, IFeedbackRepository
    {
        private readonly IticketG2dbContext _context;

        public FeedbackRepository(IUnitOfWork unitOfWork, IticketG2dbContext context) : base(unitOfWork)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Feedback> GetByIdAsync(int id)
        {
            return await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(Feedback feedback)
        {
            feedback.CreatedAt = System.DateTime.Now;
            await _context.Feedbacks.AddAsync(feedback);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.Id == id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await UnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageRatingAsync()
        {
            var ratings = await _context.Feedbacks
                .Where(f => f.Rating.HasValue)
                .Select(f => f.Rating.Value)
                .ToListAsync();

            return ratings.Count > 0 ? ratings.Average() : 0;
        }
    }
}
