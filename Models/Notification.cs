namespace suivi_abonnement.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }

        public int AbonnementId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}