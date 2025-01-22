using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
namespace suivi_abonnement.Repository
{
    public class CategorieRepository : ICategorieRepository
    {
        private readonly string connectionString;
        public CategorieRepository()
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public List<Categorie> GetCategories()
        {
            List<Categorie> categories = new List<Categorie>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM categories";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categorie categorie = new Categorie();
                                categorie.Id = reader.GetInt32("categorie_id");
                                categorie.Nom = reader.GetString("nom");
                                categories.Add(categorie);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return categories;
        }

        public Categorie SaveCategorie(Categorie categorie)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO categories (nom) VALUES (@nom)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nom", categorie.Nom);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return categorie;
        }
    }
}