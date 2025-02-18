namespace suivi_abonnement.Models
{
    public class VAbonnementClient
    {
        public int Id { get; set; }
        public string? Client { get; set; }
        public string? Email{ get; set; }
        public int AbonnementId { get; set; }
        public string? Abonnement { get; set; }
        public decimal Prix { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Fournisseur { get; set; }
        public string? Departement { get; set; }
        public string? Categorie { get; set; }

    }
}
