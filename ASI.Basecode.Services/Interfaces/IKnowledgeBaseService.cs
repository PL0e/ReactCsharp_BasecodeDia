using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IKnowledgeBaseService
    {
        Task AddAsync(KnowledgeBase article);
        IEnumerable<KnowledgeBase> GetArticles();
        KnowledgeBase GetById(int id);
        Task DeleteAsync(KnowledgeBase article);
        Task UpdateAsync(KnowledgeBase article);
    }
}
