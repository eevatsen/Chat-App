using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ChatApp.Server.Data;
using ChatApp.Server.Models;
using Azure.AI.TextAnalytics;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _context;
        private readonly TextAnalyticsClient _textAnalyticsClient;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ChatDbContext context, TextAnalyticsClient textAnalyticsClient, ILogger<ChatHub> logger)
        {
            _context = context;
            _textAnalyticsClient = textAnalyticsClient;
            _logger = logger;
        }

        public async Task SendMessage(string user, string message)
        {
            var connectionId = Context?.ConnectionId ?? "unknown";
            _logger.LogInformation("SendMessage invoked. ConnectionId={ConnectionId}, User={User}, MessageLength={Len}",
                connectionId, user ?? "<null>", message?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("SendMessage got empty message. ConnectionId={ConnectionId}, User={User}", connectionId, user);
                await Clients.Caller.SendAsync("ServerError", "Message cannot be empty.");
                return;
            }

            string msgVibe = "neutral";

            // sentiment analysis with logging
            try
            {
                DocumentSentiment documentSentiment = await _textAnalyticsClient.AnalyzeSentimentAsync(message);
                msgVibe = documentSentiment.Sentiment.ToString().ToLower();
                _logger.LogInformation("Sentiment analyzed. ConnectionId={ConnectionId}, User={User}, Sentiment={Sentiment}",
                    connectionId, user, msgVibe);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error analyzing sentiment. ConnectionId={ConnectionId}, User={User}, MessageSample={Sample}",
                    connectionId, user, message.Length > 200 ? message.Substring(0, 200) + "..." : message);
                msgVibe = "dont know";
            }

            var chatMessage = new Message
            {
                UserSent = user,
                Text = message,
                TimeSent = DateTime.UtcNow,
                Sentiment = msgVibe
            };

            try
            {
                _context.Messages.Add(chatMessage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message saved to DB. ConnectionId={ConnectionId}, User={User}, MessageId={Id}",
                    connectionId, user, chatMessage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save message to DB. ConnectionId={ConnectionId}, User={User}, MessageSample={Sample}",
                    connectionId, user, message.Length > 200 ? message.Substring(0, 200) + "..." : message);

                try
                {
                    await Clients.Caller.SendAsync("ServerError", "Unable to save message due to server error.");
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "Failed to notify caller about DB error. ConnectionId={ConnectionId}, User={User}", connectionId, user);
                }

                return;
            }

            try
            {
                await Clients.All.SendAsync("ReceiveMessage", user, message, chatMessage.TimeSent, msgVibe);
                _logger.LogInformation("Broadcasted message. ConnectionId={ConnectionId}, User={User}, Sentiment={Sentiment}",
                    connectionId, user, msgVibe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast message. ConnectionId={ConnectionId}, User={User}, MessageLength={Len}",
                    connectionId, user, message?.Length ?? 0);

                try
                {
                    await Clients.Caller.SendAsync("ServerError", "Message saved but failed to broadcast to other clients.");
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "Failed to notify caller about broadcast error. ConnectionId={ConnectionId}, User={User}", connectionId, user);
                }
            }
        }
    }
}
