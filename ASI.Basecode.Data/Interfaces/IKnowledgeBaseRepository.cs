using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IKnowledgeBaseRepository
    {
        Task AddAsync(KnowledgeBase article);
        KnowledgeBase GetById(int id);
        IEnumerable<KnowledgeBase> GetAll();
        Task UpdateAsync(KnowledgeBase article);
        Task DeleteAsync(KnowledgeBase article);
    }
}
