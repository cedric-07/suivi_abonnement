namespace suivi_abonnement.Models
{
    public class AbonnementUser
    {
        public int userId { get; set; }
        public string? userName { get; set; }
        public int TotalAbonnements { get; set; }
        public int Actifs { get; set; }
        public int Expire { get; set; }
        public int Attente { get; set; }
    }
}