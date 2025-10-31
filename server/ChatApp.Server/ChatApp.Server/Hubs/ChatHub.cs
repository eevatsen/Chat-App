using Microsoft.AspNetCore.SignalR;
using ChatApp.Server.Models;
using ChatApp.Server.Data;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string text)
        {
            Message message = new Message{
                UserSent = user,
                Text = text
            };

            await _context.AddAsync(message);
            await Clients.All.SendAsync("RecieveMessage", user, text);
        }
    }
}
