namespace suivi_abonnement.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? MessageText { get; set; }
        public DateTime DateEnvoi { get; set; }
        public bool IsRead { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int ConversationId { get; set; }
        public string? fileUrl{get; set; } 
    }
}