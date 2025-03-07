using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
using suivi_abonnement.Models;
namespace suivi_abonnement.Repository
{
    public class DepartementRepository : IDepartementRepository
    {
        private readonly string connectionString;
        public DepartementRepository()
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public List<Departement> getDepartements()
        {
            List<Departement> departements = new List<Departement>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM departements";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Departement departement = new Departement
                                {
                                    Id = reader.GetInt32("departement_id"),
                                    Nom = reader.GetString("nom")
                                };
                                departements.Add(departement);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Loggez l'erreur ou affichez un message de débogage
                Console.WriteLine($"Erreur lors de la récupération des départements: {ex.Message}");
            }
            return departements;
        }

        public List<Departement> GetDepartementsByDirection(int idDirection)
{
    List<Departement> departements = new List<Departement>();

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
        conn.Open();
        string query = "SELECT departement_id, nom FROM departements WHERE iddirection = @DirectionId";
        
        using (MySqlCommand cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@DirectionId", idDirection);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    departements.Add(new Departement
                    {
                        Id = reader.GetInt32(0),
                        Nom = reader.GetString(1)
                    });
                }
            }
        }
    }
    
    return departements;
}


    }
}