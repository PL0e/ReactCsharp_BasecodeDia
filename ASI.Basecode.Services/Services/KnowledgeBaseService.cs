using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;

        public KnowledgeBaseService(IKnowledgeBaseRepository knowledgeBaseRepository)
        {
            _knowledgeBaseRepository = knowledgeBaseRepository;
        }

        public async Task AddAsync(KnowledgeBase article)
        {
            await _knowledgeBaseRepository.AddAsync(article);
        }

        public IEnumerable<KnowledgeBase> GetArticles()
        {
            return _knowledgeBaseRepository.GetAll();
        }

        public KnowledgeBase GetById(int id)
        {
            return _knowledgeBaseRepository.GetById(id);
        }

        public async Task DeleteAsync(KnowledgeBase article)
        {
            await _knowledgeBaseRepository.DeleteAsync(article);
        }

        public async Task UpdateAsync(KnowledgeBase article)
        {
            var currentArticle = _knowledgeBaseRepository.GetById(article.Id);
            if (currentArticle == null)
                throw new Exception("Article not found.");

            currentArticle.Title = article.Title;
            currentArticle.Content = article.Content;
            currentArticle.Category = article.Category;


            await _knowledgeBaseRepository.UpdateAsync(currentArticle);
        }
    }
}
