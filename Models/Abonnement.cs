using MySql.Data.MySqlClient;
namespace suivi_abonnement.Models
{
    public class Abonnement
    {
        // Propriétés existantes
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public int Prix { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Type { get; set; }
        public int idfournisseur { get; set; }
        public int idcategorie { get; set; }
        // Nouvelles propriétés pour les catégories et les fournisseurs
        public string NomCategorie { get; set; }
        public string NomFournisseur { get; set; }
        public int idDepartement { get; set; }
        public string NomDepartement { get; set; }
    }
}
