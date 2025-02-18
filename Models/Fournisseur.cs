using MySql.Data.MySqlClient;

namespace suivi_abonnement.Models
{
    public class Fournisseur
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        
    }
}
