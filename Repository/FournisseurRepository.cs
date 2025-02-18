using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
namespace suivi_abonnement.Repository
{
    public class FournisseurRepository : IFournisseurRepository
    {
        private readonly string connectionString;
        public FournisseurRepository()
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public List<Fournisseur> GetFournisseursPaginate(int pageNumber , int pageSize)
        {
            List<Fournisseur> fournisseurs = new List<Fournisseur>();
            
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT * FROM fournisseurs LIMIT @offset, @pageSize";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pageSize", pageSize);
                        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fournisseurs.Add(new Fournisseur()
                                {
                                    Id = reader.GetInt32("fournisseur_id"),
                                    Nom = reader.GetString("nom"),
                                    Email = reader.GetString("email"),
                                    Telephone = reader.GetString("telephone")
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Fournisseur>();
            }
            return fournisseurs;
        }

        public int CountTotalOfFournisseurs()
        {
            int total = 0;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM fournisseurs";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        total = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL: {ex.Message}");
                return 0;  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                return 0;  
            }

            return total;
        }


        public List<Fournisseur> GetFournisseurs()
        {
            List<Fournisseur> fournisseurs = new List<Fournisseur>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM fournisseurs";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fournisseurs.Add(new Fournisseur()
                                {
                                    Id = reader.GetInt32("fournisseur_id"),
                                    Nom = reader.GetString("nom"),
                                    Email = reader.GetString("email"),
                                    Telephone = reader.GetString("telephone")
                                });
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Fournisseur>();
            }
            return fournisseurs;

        }
        public Fournisseur SaveFournisseur(Fournisseur fournisseur)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO fournisseurs (nom, email, telephone) VALUES (@nom, @email, @telephone)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nom", fournisseur.Nom);
                        command.Parameters.AddWithValue("@email", fournisseur.Email);
                        command.Parameters.AddWithValue("@telephone", fournisseur.Telephone);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidOperationException("Failed to update fournisseur.");
            }
            return fournisseur;
        }

        public Fournisseur UpdateFournisseur(Fournisseur fournisseur)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE fournisseurs SET nom = @nom, email = @email, telephone = @telephone WHERE fournisseur_id = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", fournisseur.Id);
                        command.Parameters.AddWithValue("@nom", fournisseur.Nom);
                        command.Parameters.AddWithValue("@email", fournisseur.Email);
                        command.Parameters.AddWithValue("@telephone", fournisseur.Telephone);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidOperationException("Failed to update fournisseur.");
            }
            return fournisseur;
        }

        public void DeleteFournisseur(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Supprimer d'abord les notifications liées aux abonnements du fournisseur
                    string deleteNotificationsQuery = @"
                        DELETE FROM notifications 
                        WHERE idabonnement IN (SELECT abonnement_id FROM abonnements WHERE idfournisseur= @id)";
                    using (var command = new MySqlCommand(deleteNotificationsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }

                    // Ensuite, supprimer les abonnements liés au fournisseur
                    string deleteAbonnementsQuery = "DELETE FROM abonnements WHERE idfournisseur = @id";
                    using (var command = new MySqlCommand(deleteAbonnementsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }

                    // Enfin, supprimer le fournisseur
                    string deleteFournisseurQuery = "DELETE FROM fournisseurs WHERE fournisseur_id = @id";
                    using (var command = new MySqlCommand(deleteFournisseurQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }


    }
}