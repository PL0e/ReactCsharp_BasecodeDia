using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITicketService
    {

        Task AddAsync(Ticket ticket);
        Ticket GetById(int id);
        IEnumerable<Ticket> GetTickets();
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(Ticket ticket);
        Task AssignTicketAsync(int ticketId, int assignedTo);

    }
}
