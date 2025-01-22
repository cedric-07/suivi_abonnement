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
                            Departement departement = new Departement();
                            departement.Id = reader.GetInt32("departement_id");
                            departement.Nom = reader.GetString("nom");
                            departements.Add(departement);

                            
                        }
                    }
                }
                connection.Close();
            }
            return departements;
        }
    }
}