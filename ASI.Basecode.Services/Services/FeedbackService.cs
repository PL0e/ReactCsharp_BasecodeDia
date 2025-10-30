using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _feedbackRepository.GetAllAsync();
        }

        public async Task<Feedback> GetByIdAsync(int id)
        {
            return await _feedbackRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Feedback feedback)
        {
            // simple validation or preprocessing can go here
            if (string.IsNullOrEmpty(feedback.Title))
                throw new System.ArgumentException("Feedback title is required.");

            if (string.IsNullOrEmpty(feedback.Content))
                throw new System.ArgumentException("Feedback content is required.");

            await _feedbackRepository.AddAsync(feedback);
        }

        public async Task DeleteAsync(int id)
        {
            await _feedbackRepository.DeleteAsync(id);
        }

        public async Task<double> GetAverageRatingAsync()
        {
            return await _feedbackRepository.GetAverageRatingAsync();
        }
    }
}
