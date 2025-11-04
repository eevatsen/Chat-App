using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Server.Models
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeSent { get; set; }
        public string UserSent { get; set; }
        public string? Sentiment { get; set; }
    }
}
