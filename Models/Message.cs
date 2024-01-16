namespace Hospital_Software.Models
{
    public class Message
    {
        public string Id { get; set; }

        public string SenderId { get; set; }

        public string ReceivingId { get; set; }

        public DateTime Time { get; set; }

        public string Content { get; set; }

        public bool Read { get; set; }
    }
}
