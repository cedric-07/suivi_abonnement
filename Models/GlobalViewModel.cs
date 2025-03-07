namespace suivi_abonnement.Models
{
    public class GlobalViewModel
    {
        public AbonnementStatViewModel? AbonnementStatViewModel { get; set; }
        public AbonnementViewModel? AbonnementViewModel { get; set; }
        public FournisseurViewModel? FournisseurViewModel { get; set; }
        public HistoriqueViewModel? HistoriqueViewModel { get; set; }
        public List<VAbonnementClient>? VAbonnementClients { get; set; }
        public MessageViewModel? MessageViewModel { get; set; }
        public List<Dictionary<string, object>>? AbonnementsParUser { get; set; } // Ajout de la liste des abonnements par utilisateur
    }
}
