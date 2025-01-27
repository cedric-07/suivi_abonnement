namespace suivi_abonnement.Models
{
    public class AbonnementStatViewModel
    {
        public int Actifs { get; set; }
        public int Expirés { get; set; }
        public int Suspendus { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<Abonnement> listeAbonnementActif  { get; set; }
        public List<Abonnement> listeAbonnementExpiré { get; set; }
        public List<Abonnement> listeAbonnementEnAttente { get; set; }
        public List<Dictionary<string, object>> RevenusAnnuels { get; set; }
        public List<Dictionary<string, object>> RevenusMensuels { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

}
