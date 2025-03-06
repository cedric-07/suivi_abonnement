using MySql.Data.MySqlClient;
using suivi_abonnement.Models;
using System;
using suivi_abonnement.Repository.Interface;
namespace suivi_abonnement.Repository
{
    public class DirectionRepository: IDirectionRepository
    {
        private string connectionString;
        public DirectionRepository()
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public List<Direction> GetDirections()
        {
            List<Direction> directions = new List<Direction>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM directions";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                directions.Add(new Direction()
                                {
                                    Id = Convert.ToInt32(reader["direction_id"]),
                                    Nom = reader["nom"].ToString(),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return directions;
        }
    }
}