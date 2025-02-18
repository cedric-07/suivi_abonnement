namespace suivi_abonnement.Models
{
    public class VStatusAbonnement
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int Prix { get; set; }
        public string? Status { get; set; }
        public string? Categorie { get; set; }
        public string? Fournisseur { get; set; }
        public string? Departement { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }
}