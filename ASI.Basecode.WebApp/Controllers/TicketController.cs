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
using System.Text.Json;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [AllowAnonymous]
    public class TicketController : ControllerBase<TicketController>
    {
        private readonly ITicketService _ticketService;

        public TicketController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ITicketService ticketService
        ) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] Ticket ticket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ticket.CreatedAt = DateTime.Now;

            await _ticketService.AddAsync(ticket);

            return Ok(new
            {
                message = "Ticket created successfully!",
                ticket
            });
        }

        [HttpGet]
        public IActionResult GetTickets()
        {
            var tickets = _ticketService.GetTickets();
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var ticket = _ticketService.GetById(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            return Ok(ticket);
        }


        [HttpPatch("{id}")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("assignedTo", out var assignedToElement))
                {
                    return BadRequest(new { message = "assignedTo is required." });
                }

                var assignedTo = assignedToElement.GetInt32();

                await _ticketService.AssignTicketAsync(id, assignedTo);

                return Ok(new { message = "Ticket assigned successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = _ticketService.GetById(id);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            await _ticketService.DeleteAsync(ticket);
            return Ok(new { message = "Ticket deleted successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] Ticket updatedTicket)
        {
            if (updatedTicket == null || updatedTicket.Id != id)
                return BadRequest(new { message = "Invalid ticket data." });

            try
            {
                await _ticketService.UpdateAsync(updatedTicket);
                return Ok(new { message = "Ticket updated successfully!" });
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
