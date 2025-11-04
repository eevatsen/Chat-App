using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Data;
using ChatApp.Server.Models;

namespace ChatApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public MessagesController(ChatDbContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages
                .OrderBy(m => m.TimeSent) 
                .Select(m => new
                {
                    id = m.Id,
                    user = m.UserSent,
                    text = m.Text,
                    timesent = m.TimeSent,    
                    sentiment = m.Sentiment
                })
                .ToListAsync();

            return Ok(messages);
        }

        // GET: api/Messages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAllMessages()
        {
            var allMessages = _context.Messages;
            _context.Messages.RemoveRange(allMessages);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
