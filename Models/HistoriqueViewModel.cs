namespace suivi_abonnement.Models
{
    public class HistoriqueViewModel
    {
        public List<VAbonnementClient> Abonnements { get; set; }
        public int TotalAbonnements { get; set; }
        public int NbrClientAbonne { get; set; }
        /// <summary>
    
        /// </summary>
        public List<VStatusAbonnement> Actifs { get; set; }
        public List<VStatusAbonnement> Expirés { get; set; }
        public List<VStatusAbonnement> EnAttente { get; set; }
        public int TotalPagesActifs { get; set; }
        public int TotalPagesExpirés { get; set; }
        public int TotalPagesEnAttente { get; set; }
        public int CurrentPageActifs { get; set; }
        public int CurrentPageExpirés { get; set; }
        public int CurrentPageEnAttente { get; set; }
        public int TotalActifs { get; set; }
        public int TotalExpires { get; set; }
        public int TotalEnAttente { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int Prix { get; set; }
        public string Categorie { get; set; }
        public string Fournisseur { get; set; }
        public string Departement { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Status { get; set; }
    }
}