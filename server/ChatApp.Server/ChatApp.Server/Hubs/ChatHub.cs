using Microsoft.AspNetCore.SignalR;
using ChatApp.Server.Data; 
using ChatApp.Server.Models; 

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new Message
            {
                UserSent = user,
                Text = message, 
                TimeSent = DateTime.UtcNow
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();


            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}