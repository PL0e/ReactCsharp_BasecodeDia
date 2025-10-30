using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket);
        Ticket GetById(int id);
        IEnumerable<Ticket> GetAll();
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(Ticket ticket);
        Task AssignTicketAsync(int ticketId, int assignedTo);

    }
}
