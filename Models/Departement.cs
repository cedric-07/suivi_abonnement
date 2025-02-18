using System.Configuration;
using MySql.Data.MySqlClient;

namespace suivi_abonnement
{
    public class Departement
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        
    }
}