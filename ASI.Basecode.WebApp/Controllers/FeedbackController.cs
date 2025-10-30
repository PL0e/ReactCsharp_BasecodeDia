using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class FeedbackController : ControllerBase<FeedbackController>
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IFeedbackService feedbackService
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var feedback = await _feedbackService.GetByIdAsync(id);
            if (feedback == null)
                return NotFound(new { message = "Feedback not found." });

            return Ok(feedback);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] Feedback feedback)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _feedbackService.AddAsync(feedback);
            return Ok(new { message = "Feedback submitted successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            await _feedbackService.DeleteAsync(id);
            return Ok(new { message = "Feedback deleted successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAverageRating()
        {
            var avg = await _feedbackService.GetAverageRatingAsync();
            return Ok(new { averageRating = avg });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
