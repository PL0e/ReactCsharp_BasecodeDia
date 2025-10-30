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
    public class KnowledgeBaseController : ControllerBase<KnowledgeBaseController>
    {
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public KnowledgeBaseController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IKnowledgeBaseService knowledgeBaseService
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _knowledgeBaseService = knowledgeBaseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle([FromBody] KnowledgeBase article)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            article.CreatedAt = DateTime.Now;

            await _knowledgeBaseService.AddAsync(article);

            return Ok(new
            {
                message = "Article created successfully!",
                article
            });
        }

        [HttpGet]
        public IActionResult GetArticles()
        {
            var articles = _knowledgeBaseService.GetArticles();
            return Ok(articles);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var article = _knowledgeBaseService.GetById(id);
            if (article == null)
                return NotFound(new { message = "Article not found." });

            return Ok(article);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = _knowledgeBaseService.GetById(id);
            if (article == null)
            {
                return NotFound(new { message = "Article not found." });
            }

            await _knowledgeBaseService.DeleteAsync(article);
            return Ok(new { message = "Article deleted successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] KnowledgeBase updatedArticle)
        {
            if (updatedArticle == null || updatedArticle.Id != id)
                return BadRequest(new { message = "Invalid article data." });

            try
            {
                await _knowledgeBaseService.UpdateAsync(updatedArticle);
                return Ok(new { message = "Article updated successfully!" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
