using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace suivi_abonnement.Repository
{
    public class AbonnementRepository : IAbonnementRepository
    {
        private readonly string _connectionString;
        public AbonnementRepository()
        {
            _connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        //ADMINISTRATEUR
        public List<Abonnement> getAbonnements(int pageNumber, int pageSize)
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur , d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                LIMIT @offset, @pageSize";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pageSize", pageSize);
                        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return abonnements;
        }



        public int CountTotalAbonnements()
        {
            int count = 0;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) AS total FROM abonnements";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return count;
        }

        //ADMINISTRATEUR
        public Abonnement SaveAbonnement(Abonnement newAbonnement)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                INSERT INTO abonnements (nom, description, prix, date_debut, expiration_date, type, idfournisseur, idcategorie , departement_id)
                                VALUES (@nom, @description, @prix, @date_debut, @expiration_date, @type, @idfournisseur, @idcategorie , @departement_id)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nom", newAbonnement.Nom);
                        command.Parameters.AddWithValue("@description", newAbonnement.Description);
                        command.Parameters.AddWithValue("@prix", newAbonnement.Prix);
                        command.Parameters.AddWithValue("@date_debut", newAbonnement.DateDebut);
                        command.Parameters.AddWithValue("@expiration_date", newAbonnement.ExpirationDate);
                        command.Parameters.AddWithValue("@type", newAbonnement.Type);
                        command.Parameters.AddWithValue("@idfournisseur", newAbonnement.idfournisseur);
                        command.Parameters.AddWithValue("@idcategorie", newAbonnement.idcategorie);
                        command.Parameters.AddWithValue("@departement_id", newAbonnement.idDepartement);
                        command.ExecuteNonQuery();


                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return newAbonnement;
        }
        public List<Abonnement> GetAbonnementById(int id)
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur
                                FROM abonnements a
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE abonnement_id = @id";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //ADMINISTRATEUR
        public string updateAbonnement(Abonnement abonnement)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                UPDATE abonnements
                SET nom = @nom, description = @description, prix = @prix, date_debut = @date_debut, expiration_date = @expiration_date, type = @type, idfournisseur = @idfournisseur, idcategorie = @idcategorie , departement_id = @departement_id
                WHERE abonnement_id = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", abonnement.Id);
                        command.Parameters.AddWithValue("@nom", abonnement.Nom);
                        command.Parameters.AddWithValue("@description", abonnement.Description);
                        command.Parameters.AddWithValue("@prix", abonnement.Prix);
                        command.Parameters.AddWithValue("@date_debut", abonnement.DateDebut);
                        command.Parameters.AddWithValue("@expiration_date", abonnement.ExpirationDate);
                        command.Parameters.AddWithValue("@type", abonnement.Type);
                        command.Parameters.AddWithValue("@idfournisseur", abonnement.idfournisseur);
                        command.Parameters.AddWithValue("@idcategorie", abonnement.idcategorie);
                        command.Parameters.AddWithValue("@departement_id", abonnement.idDepartement);

                        // Log des paramètres pour vérifier
                        Console.WriteLine($"ID: {abonnement.Id}, Nom: {abonnement.Nom}, Description: {abonnement.Description}, Prix: {abonnement.Prix} , Date Debut: {abonnement.DateDebut}, Expiration Date: {abonnement.ExpirationDate}, Type: {abonnement.Type}, ID Fournisseur: {abonnement.idfournisseur}, ID Categorie: {abonnement.idcategorie}");

                        var result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            return "Abonnement mis à jour avec succès";
                        }
                        else
                        {
                            return "Abonnement non mis à jour";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Erreur lors de la mise à jour : " + ex.Message;
            }
        }

        //ADMINISTRATEUR
        public string deleteAbonnement(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Commencer une transaction
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Supprimer les notifications associées
                            string deleteNotificationsQuery = "DELETE FROM notifications WHERE idabonnement = @id";
                            using (var deleteNotificationsCommand = new MySqlCommand(deleteNotificationsQuery, connection, transaction))
                            {
                                deleteNotificationsCommand.Parameters.AddWithValue("@id", id);
                                deleteNotificationsCommand.ExecuteNonQuery();
                            }

                            // Supprimer l'abonnement
                            string deleteAbonnementQuery = "DELETE FROM abonnements WHERE abonnement_id = @id";
                            using (var deleteAbonnementCommand = new MySqlCommand(deleteAbonnementQuery, connection, transaction))
                            {
                                deleteAbonnementCommand.Parameters.AddWithValue("@id", id);
                                var result = deleteAbonnementCommand.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    // Valider la transaction
                                    transaction.Commit();
                                    return "Abonnement supprimé avec succès.";
                                }
                                else
                                {
                                    // Annuler la transaction si l'abonnement n'existe pas
                                    transaction.Rollback();
                                    return "L'abonnement n'a pas été trouvé.";
                                }
                            }
                        }
                        catch
                        {
                            // Annuler la transaction en cas d'erreur
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la suppression : " + ex.Message);
                return "Erreur lors de la suppression de l'abonnement.";
            }
        }

        //ADMINISTRATEUR
        public List<Abonnement> searchMultiplyAbonnement(string keyword)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur , d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE a.nom LIKE @keyword OR a.description LIKE @keyword OR c.nom LIKE @keyword OR f.nom LIKE @keyword OR a.type LIKE @keyword";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //ADMINISTRATEUR
        //filtre par date
        public List<Abonnement> FiltrePerDate(DateTime date_debut, DateTime expiration_date)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                // Affichage des dates obtenues
                Console.WriteLine("Date de début : " + date_debut.ToString("yyyy-MM-dd HH:mm"));
                Console.WriteLine("Date d'expiration : " + expiration_date.ToString("yyyy-MM-dd HH:mm"));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // Modifiez la requête SQL comme suggéré ci-dessus
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur, d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE (date_debut BETWEEN @date_debut AND @expiration_date
                                OR expiration_date BETWEEN @date_debut AND @expiration_date)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Paramétrage des dates pour éviter les injections SQL
                        command.Parameters.AddWithValue("@date_debut", date_debut);
                        command.Parameters.AddWithValue("@expiration_date", expiration_date);


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnements.Add(abonnement);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message); // Améliorer l'affichage de l'erreur pour plus de détails
            }

            return abonnements;
        }

        //ADMINISTRATEUR
        //filtre par categorie
        public List<Abonnement> FiltrePerCategorie(int idcategorie)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur, d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE a.idcategorie = @idcategorie";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idcategorie", idcategorie);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //ADMINISTRATEUR
        //filtre par type
        public List<Abonnement> FiltrePerType(string type)
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur , d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE a.type = @type";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@type", type);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //ADMINISTRATEUR
        //Statistique par abonnement actif
        public int CountTotalAbonnementsActif()
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) AS total FROM abonnements WHERE expiration_date > NOW() AND date_debut < NOW()";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return count;
        }

        //ADMINISTRATEUR
        //Statistique par abonnement inactif
        public int CountTotalAbonnementsInactif()
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) AS total FROM abonnements WHERE expiration_date < NOW()";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return count;
        }

        //ADMINISTRATEUR
        //Statistique par abonnement en attente
        public int CountTotalAbonnementsEnAttente()
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) AS total FROM abonnements WHERE date_debut > NOW()";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return count;
        }

        //ADMINISTRATEUR
        //Calculer les revenus fictifs des abonnements par année (valeurs simulées)
        public List<Dictionary<string, object>> RevenusFictifsParAnnee()
        {
            List<Dictionary<string, object>> revenus = new List<Dictionary<string, object>>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT YEAR(date_debut) AS annee, SUM(prix) AS revenus
                                FROM abonnements
                                GROUP BY YEAR(date_debut)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> revenu = new Dictionary<string, object>();
                                revenu.Add("annee", reader.GetInt32("annee"));
                                revenu.Add("revenus", reader.GetInt32("revenus"));
                                revenus.Add(revenu);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return revenus;
        }

        //ADMINISTRATEUR
        //Calculer les revenus fictifs des abonnements par mois (valeurs simulées)
        public List<Dictionary<string, object>> RevenusFictifsParMois()
        {
            List<Dictionary<string, object>> revenus = new List<Dictionary<string, object>>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT YEAR(date_debut) AS annee, MONTH(date_debut) AS mois, SUM(prix) AS revenus
                                FROM abonnements
                                GROUP BY YEAR(date_debut), MONTH(date_debut)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> revenu = new Dictionary<string, object>();
                                revenu.Add("annee", reader.GetInt32("annee"));
                                revenu.Add("mois", reader.GetInt32("mois"));
                                revenu.Add("revenus", reader.GetInt32("revenus"));
                                revenus.Add(revenu);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return revenus;
        }
        

        //CLIENT
        public List<Abonnement> getAbonnementByUser(int pageNumber, int pageSize, int userId)
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                                SELECT 
                                    a.abonnement_id,
                                    a.type,
                                    a.prix,
                                    a.date_debut,
                                    a.expiration_date,
                                    a.description,
                                    a.idfournisseur,
                                    a.idcategorie,
                                    c.nom AS nom_categorie,
                                    f.nom AS nom_fournisseur,
                                    a.nom AS abonnement_nom,
                                    d.departement_id,
                                    d.nom AS departement_nom
                                FROM 
                                    abonnements a
                                JOIN 
                                    categories c ON a.idcategorie = c.categorie_id
                                JOIN
                                    fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                JOIN 
                                    departements d ON a.departement_id = d.departement_id
                                JOIN 
                                    departement_user du ON d.departement_id = du.iddepartement
                                WHERE 
                                    du.user_id = @UserId

                                LIMIT @offset, @pageSize";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pageSize", pageSize);
                        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
                        command.Parameters.AddWithValue("@userId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("abonnement_nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnement.idDepartement = reader.GetInt32("departement_id");
                                abonnement.NomDepartement = reader.GetString("departement_nom");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return abonnements;
        }

        //recherche par mot clé
        public List<Abonnement> searchMultiplyMot(string keyword, int userId)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur, d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE (a.nom LIKE @keyword OR a.description LIKE @keyword OR c.nom LIKE @keyword OR f.nom LIKE @keyword OR a.type LIKE @keyword)
                                AND a.departement_id = (SELECT iddepartement FROM departement_user WHERE user_id = @userId);
                                ";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //filtre par date
        public List<Abonnement> FiltreDate(DateTime date_debut, DateTime expiration_date, int userId)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                // Affichage des dates obtenues
                Console.WriteLine("Date de début : " + date_debut.ToString("yyyy-MM-dd HH:mm"));
                Console.WriteLine("Date d'expiration : " + expiration_date.ToString("yyyy-MM-dd HH:mm"));

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // Modifiez la requête SQL comme suggéré ci-dessus
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur, d.nom AS nom_departement 
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE (date_debut BETWEEN @date_debut AND @expiration_date
                                OR expiration_date BETWEEN @date_debut AND @expiration_date) AND a.departement_id = (SELECT iddepartement FROM departement_user WHERE user_id = @userId)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Paramétrage des dates pour éviter les injections SQL
                        command.Parameters.AddWithValue("@date_debut", date_debut);
                        command.Parameters.AddWithValue("@expiration_date", expiration_date);
                        command.Parameters.AddWithValue("@userId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnements.Add(abonnement);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message); // Améliorer l'affichage de l'erreur pour plus de détails
            }

            return abonnements;
        }

        //filtre par categorie
        public List<Abonnement> FiltreCategorie(int idcategorie, int userId)
        {
            List<Abonnement> abonnements = new List<Abonnement>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur, d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE a.idcategorie = @idcategorie AND a.departement_id = (SELECT iddepartement FROM departement_user WHERE user_id = @userId)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idcategorie", idcategorie);
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //filtre par type
        public List<Abonnement> FiltreType(string type, int userId)
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                                SELECT a.*, c.nom AS nom_categorie, f.nom AS nom_fournisseur , d.nom AS nom_departement
                                FROM abonnements a
                                JOIN departements d ON a.departement_id = d.departement_id
                                JOIN categories c ON a.idcategorie = c.categorie_id
                                JOIN fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                WHERE a.type = @type AND a.departement_id = (SELECT iddepartement FROM departement_user WHERE user_id = @userId)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@type", type);
                        command.Parameters.AddWithValue("@userId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Abonnement abonnement = new Abonnement();
                                abonnement.Id = reader.GetInt32("abonnement_id");
                                abonnement.Nom = reader.GetString("nom");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.ExpirationDate = reader.GetDateTime("expiration_date");
                                abonnement.Type = reader.GetString("type");
                                abonnement.idfournisseur = reader.GetInt32("idfournisseur");
                                abonnement.idcategorie = reader.GetInt32("idcategorie");
                                abonnement.NomDepartement = reader.GetString("nom_departement");
                                abonnement.NomCategorie = reader.GetString("nom_categorie");
                                abonnement.NomFournisseur = reader.GetString("nom_fournisseur");
                                abonnements.Add(abonnement); // Ajout de l'abonnement à la liste
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return abonnements;
        }

        //ADMIN
        public List<VAbonnementClient> getListVAbonnement(int pageNumber, int pageSize)
        {
            List<VAbonnementClient> abonnementList = new List<VAbonnementClient>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM v_abonnements_par_client WHERE roleclient = 'user' LIMIT @offset , @pageSize ";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                VAbonnementClient abonnement = new VAbonnementClient();
                                abonnement.Id = reader.GetInt32("idclient");
                                abonnement.Client = reader.GetString("nomclient");
                                abonnement.Email = reader.GetString("emailclient");
                                abonnement.AbonnementId = reader.GetInt32("abonnement_id");
                                abonnement.Abonnement = reader.GetString("nomabonnement");
                                abonnement.Prix = reader.GetInt32("prix");
                                abonnement.DateDebut = reader.GetDateTime("date_debut");
                                abonnement.DateFin = reader.GetDateTime("expiration_date");
                                abonnement.Description = reader.GetString("description");
                                abonnement.Type = reader.GetString("type");
                                abonnement.Fournisseur = reader.GetString("nomfournisseur");
                                abonnement.Departement = reader.GetString("nomdepartement");
                                abonnementList.Add(abonnement);
                            }
                        }
                    }
                    connection.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return abonnementList;

        }

        public int CountTotalVAbonnement()
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) AS total FROM v_abonnements_par_client WHERE roleclient = 'user'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return count;

        }

        public int NbrClientAbonne()
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(DISTINCT idclient) AS total FROM v_abonnements_par_client WHERE roleclient = 'user'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return count;
        }

        public (List<VStatusAbonnement> actifs, List<VStatusAbonnement> enAttente, List<VStatusAbonnement> expires) getListAbonnementByStatus(int pageNumberActifs , int pageNumberEnAttente, int pageNumberExpires, int pageSize)
        {
            // Initialisation des listes pour chaque statut
            List<VStatusAbonnement> actifs = new List<VStatusAbonnement>();
            List<VStatusAbonnement> enAttente = new List<VStatusAbonnement>();
            List<VStatusAbonnement> expires = new List<VStatusAbonnement>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    
                    // Requête SQL avec un filtre de statut
                    string query = @"
                        SELECT * 
                        FROM v_status_abonnements
                        WHERE statut = 'Actif'
                        LIMIT @offsetActif, @pageSize";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@offsetActif", (pageNumberActifs - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                actifs.Add(new VStatusAbonnement
                                {
                                    Id = reader.GetInt32("abonnement_id"),
                                    Nom = reader.GetString("nom"),
                                    Description = reader.GetString("description"),
                                    Prix = reader.GetInt32("prix"),
                                    Status = reader.GetString("statut"),
                                    DateDebut = reader.GetDateTime("date_debut"),
                                    DateFin = reader.GetDateTime("expiration_date"),
                                    Type = reader.GetString("type"),
                                    Departement = reader.GetString("nom_departement"),
                                    Categorie = reader.GetString("nom_categorie"),
                                    Fournisseur = reader.GetString("nom_fournisseur")
                                });
                            }
                        }
                    }

                    string queryEnAttente = @"
                        SELECT * 
                        FROM v_status_abonnements
                        WHERE statut = 'En attente'
                        LIMIT @offsetenAttente, @pageSize";

                    using (var command = new MySqlCommand(queryEnAttente, connection))
                    {
                        command.Parameters.AddWithValue("@offsetenAttente", (pageNumberEnAttente - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                enAttente.Add(new VStatusAbonnement
                                {
                                    Id = reader.GetInt32("abonnement_id"),
                                    Nom = reader.GetString("nom"),
                                    Description = reader.GetString("description"),
                                    Prix = reader.GetInt32("prix"),
                                    Status = reader.GetString("statut"),
                                    DateDebut = reader.GetDateTime("date_debut"),
                                    DateFin = reader.GetDateTime("expiration_date"),
                                    Type = reader.GetString("type"),
                                    Departement = reader.GetString("nom_departement"),
                                    Categorie = reader.GetString("nom_categorie"),
                                    Fournisseur = reader.GetString("nom_fournisseur")
                                });
                            }
                        }
                    }

                    string queryExpires = @"
                        SELECT *
                        FROM v_status_abonnements
                        WHERE statut = 'Expiré'
                        LIMIT @offsetExpires, @pageSize";

                    using (var command = new MySqlCommand(queryExpires, connection))
                    {
                        command.Parameters.AddWithValue("@offsetExpires", (pageNumberExpires - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                expires.Add(new VStatusAbonnement
                                {
                                    Id = reader.GetInt32("abonnement_id"),
                                    Nom = reader.GetString("nom"),
                                    Description = reader.GetString("description"),
                                    Prix = reader.GetInt32("prix"),
                                    Status = reader.GetString("statut"),
                                    DateDebut = reader.GetDateTime("date_debut"),
                                    DateFin = reader.GetDateTime("expiration_date"),
                                    Type = reader.GetString("type"),
                                    Departement = reader.GetString("nom_departement"),
                                    Categorie = reader.GetString("nom_categorie"),
                                    Fournisseur = reader.GetString("nom_fournisseur")
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
            }
            // Retourner les listes d'abonnements filtrées par statut
            return (actifs, enAttente, expires);
        }

        public List<Abonnement> getAbonnementsExpiredOnMonth()
        {
            List<Abonnement> abonnements = new List<Abonnement>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                        a.*, 
                                        c.nom AS nom_categorie, 
                                        f.nom AS nom_fournisseur, 
                                        d.nom AS nom_departement 
                                    FROM 
                                        abonnements a 
                                    JOIN
                                        departements d ON a.departement_id = d.departement_id
                                    JOIN
                                        categories c ON a.idcategorie = c.categorie_id
                                    JOIN
                                        fournisseurs f ON a.idfournisseur = f.fournisseur_id
                                    WHERE 
                                        DATEDIFF(expiration_date , CURDATE()) <= 30 
                                    AND 
                                        DATEDIFF(expiration_date , CURDATE()) >= 0";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                abonnements.Add(new Abonnement
                                {
                                    Id = reader.GetInt32("abonnement_id"),
                                    Nom = reader.GetString("nom"),
                                    Description = reader.GetString("description"),
                                    Prix = reader.GetInt32("prix"),
                                    DateDebut = reader.GetDateTime("date_debut"),
                                    ExpirationDate = reader.GetDateTime("expiration_date"),
                                    Type = reader.GetString("type"),
                                    idfournisseur = reader.GetInt32("idfournisseur"),
                                    idcategorie = reader.GetInt32("idcategorie"),
                                    idDepartement = reader.GetInt32("departement_id"),
                                    NomCategorie = reader.GetString("nom_categorie"),
                                    NomDepartement = reader.GetString("nom_departement"),
                                    NomFournisseur = reader.GetString("nom_fournisseur")
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
            }
            return abonnements;
        }
    }
}