using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task AddAsync(Ticket ticket)
        {
            await _ticketRepository.AddAsync(ticket);
        }

        public Ticket GetById(int id)
        {
            return _ticketRepository.GetById(id);
        }
        
        public IEnumerable<Ticket> GetTickets()
        {
            return _ticketRepository.GetAll();
        }

        public async Task DeleteAsync(Ticket ticket)
        {
            await _ticketRepository.DeleteAsync(ticket);
        }

        public async Task AssignTicketAsync(int ticketId, int assignedTo)
        {
            await _ticketRepository.AssignTicketAsync(ticketId, assignedTo);
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            var currentTicket = _ticketRepository.GetById(ticket.Id);
            if (currentTicket == null)
                throw new Exception("Ticket not found.");

            currentTicket.Title = ticket.Title;
            currentTicket.Description = ticket.Description;
            currentTicket.Category = ticket.Category;
            currentTicket.Priority = ticket.Priority;
            currentTicket.Attachment = ticket.Attachment;
            currentTicket.AssignedTo = ticket.AssignedTo;
            currentTicket.DueDate = ticket.DueDate;
            currentTicket.Status = ticket.Status;

            await _ticketRepository.UpdateAsync(currentTicket);
        }

    }
}

