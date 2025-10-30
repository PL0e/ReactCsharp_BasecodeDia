using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class KnowledgeBaseRepository : BaseRepository, IKnowledgeBaseRepository
    {
        public KnowledgeBaseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task AddAsync(KnowledgeBase article)
        {
            await this.GetDbSet<KnowledgeBase>().AddAsync(article);
            await UnitOfWork.SaveChangesAsync();
        }

        public KnowledgeBase GetById(int id)
        {
            return this.GetDbSet<KnowledgeBase>().FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<KnowledgeBase> GetAll()
        {
            return this.GetDbSet<KnowledgeBase>().ToList();
        }

        public async Task UpdateAsync(KnowledgeBase article)
        {
            this.GetDbSet<KnowledgeBase>().Update(article);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(KnowledgeBase article)
        {
            this.GetDbSet<KnowledgeBase>().Remove(article);
            await UnitOfWork.SaveChangesAsync();

        }
    }
}
