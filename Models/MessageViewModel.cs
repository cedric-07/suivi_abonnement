namespace suivi_abonnement.Models
{
    public class MessageViewModel
    {
        public int? ReceiverId { get; set; } // Id du destinataire
        public List<User> Users { get; set; } // Liste des utilisateurs
        public List<User> adminUser {get; set; }
        public int CurrentUserId { get; set; } // Id de l'utilisateur actuel
        public List<Message> Messages { get; set; } // Liste des messages
        public User? User { get; set; } // Utilisateur
        public MessageViewModel()
        {
            Users = new List<User>();
            adminUser = new List<User>();
            Messages = new List<Message>();
        }
    }
}