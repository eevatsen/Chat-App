using Microsoft.AspNetCore.SignalR;
using ChatApp.Server.Data; 
using ChatApp.Server.Models;
using Azure;
using Azure.AI.TextAnalytics;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;
        private readonly TextAnalyticsClient _textAnalyticsClient;
        public ChatHub(ChatDbContext context, TextAnalyticsClient textAnalyticsClient)
        {
            _context = context;
            _textAnalyticsClient = textAnalyticsClient;
        }

        public async Task SendMessage(string user, string message)
        {
            string msgVibe = "neutral";

            try
            {
                DocumentSentiment documentSentiment = await _textAnalyticsClient.AnalyzeSentimentAsync(message);
                msgVibe = documentSentiment.Sentiment.ToString().ToLower();
            }
            catch
            {
                Console.WriteLine($"Error analyzing sentiment: {message}");
                msgVibe = "dont know";
            }

            var chatMessage = new Message
            {
                UserSent = user,
                Text = message,
                TimeSent = DateTime.UtcNow,
                Sentiment = msgVibe
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();


            await Clients.All.SendAsync("ReceiveMessage", user, message, msgVibe);
        }
    }
}