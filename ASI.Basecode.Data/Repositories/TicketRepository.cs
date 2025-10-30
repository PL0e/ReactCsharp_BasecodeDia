using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;

namespace ASI.Basecode.Data.Repositories
{
    public class TicketRepository: BaseRepository, ITicketRepository
    {
        public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task AddAsync(Ticket ticket)
        {
            await this.GetDbSet<Ticket>().AddAsync(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        public Ticket GetById(int id)
        {
            return this.GetDbSet<Ticket>().FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<Ticket> GetAll()
        {
            return this.GetDbSet<Ticket>().ToList();
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            this.GetDbSet<Ticket>().Update(ticket);
            await UnitOfWork.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(Ticket ticket)
        { 
            this.GetDbSet<Ticket>().Remove(ticket);
            await UnitOfWork.SaveChangesAsync(); 
        
        }

        public async Task AssignTicketAsync(int ticketId, int assignedTo)
        {
            var ticket = await this.GetDbSet<Ticket>().FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.AssignedTo = assignedTo;
                await UnitOfWork.SaveChangesAsync();
            }
        }

    }
}
