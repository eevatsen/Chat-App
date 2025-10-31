namespace ChatApp.Server.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeSent { get; set; }
        public string UserSent { get; set; }
    }
}
