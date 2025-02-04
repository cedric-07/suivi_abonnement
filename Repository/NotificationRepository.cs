using suivi_abonnement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
namespace suivi_abonnement.Service
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly string connectionString;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public void SendNotification()
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (userRole == "admin")
                    {
                        SendNotificationByRoleAdmin("admin");
                    }
                    else if (userRole == "user")
                    {
                        SendNotificationByRoleUser( "user");
                    }
                    else
                    {
                        throw new Exception("Role non reconnu");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void SendNotificationByRoleAdmin(string role)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Notification pour abonnements expirant dans 30 et 7 jours
                    string query = @"SELECT 
                                        a.abonnement_id,
                                        a.nom,
                                        u.id AS iduser,
                                        u.username,
                                        u.email,
                                        DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants
                                    FROM abonnements a
                                    JOIN users u ON u.role = @role
                                    WHERE DATEDIFF(a.expiration_date, CURDATE()) BETWEEN 0 AND 30";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@role", role);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int userId = reader.GetInt32("iduser");
                                    int abonnementId = reader.GetInt32("abonnement_id");
                                    int joursRestants = reader.GetInt32("jours_restants");
                                    string nomAbonnement = reader.GetString("nom");
                                    string nomClient = reader.GetString("username");
                                    string emailClient = reader.GetString("email");

                                    string message;
                                    string type;

                                    if (joursRestants <= 7)
                                    {
                                        message = $"L'abonnement {nomAbonnement} va expirer dans {joursRestants} jour(s)";
                                        type = "Rappel";
                                    }
                                    else
                                    {
                                        message = $"L'abonnement {nomAbonnement} va expirer dans {joursRestants} jours";
                                        type = "Alerte";
                                    }

                                    try
                                    {
                                        CreateNotification(userId, abonnementId, message, type); // Utilisation du type dynamique
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erreur lors de la création de la notification pour {userId}: {ex.Message}");
                                        Console.WriteLine(ex.StackTrace);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Aucune donnée trouvée pour la requête.");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Une erreur s'est produite lors de l'envoi des notifications : " + e.Message);
            }
        }




        public void SendNotificationByRoleUser(string role)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Notification pour abonnements expirant dans 30 et 7 jours
                    string query = @"SELECT idclient , abonnement_id , nomabonnement , nomclient , emailclient , idclient , DATEDIFF(expiration_date, CURDATE()) AS jours_restants 
                                    FROM v_abonnements_par_client WHERE roleclient = @role AND DATEDIFF(expiration_date, CURDATE()) BETWEEN 0 AND 30";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@role", role);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("idclient");
                                int abonnementId = reader.GetInt32("abonnement_id");
                                int joursRestants = reader.GetInt32("jours_restants");
                                string nomAbonnement = reader.GetString("nomabonnement");
                                string nomClient = reader.GetString("nomclient");
                                string emailClient = reader.GetString("emailclient");

                                string message;
                                string type;

                                if (joursRestants <= 7)
                                {
                                    message = $"L'abonnement {nomAbonnement} de {nomClient} va expirer dans {joursRestants} jour(s)";
                                    type = "Rappel";
                                }
                                else
                                {
                                    message = $"L'abonnement {nomAbonnement} de {nomClient} va expirer dans {joursRestants} jours";
                                    type = "Alerte";
                                }

                                try
                                {
                                    CreateNotification(userId, abonnementId, message, type); // Utilisation du type dynamique
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Erreur pour {userId}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Une erreur s'est produite lors de l'envoi des notifications : " + e.Message);
            }
        }




            public void CreateNotification(int userId, int abonnementId, string message , string type)
            {
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // Vérification si la notification existe déjà
                        string checkAbonnementIsExist = "SELECT COUNT(*) FROM notifications WHERE idabonnement = @idabonnement AND iduser = @iduser";
                        using (var checkcommand = new MySqlCommand(checkAbonnementIsExist, connection))
                        {
                            checkcommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                            checkcommand.Parameters.AddWithValue("@iduser", userId);

                            int AbonnementexistingToNotify = Convert.ToInt32(checkcommand.ExecuteScalar());

                            if (AbonnementexistingToNotify > 0)
                            {
                                return; // Sortie de la méthode si la notification existe déjà
                            }
                        }

                        // Insertion de la nouvelle notification
                        string insertquery = "INSERT INTO notifications (message, type, status, idabonnement, iduser, created_at) " +
                                            "VALUES (@message, @type, 'non lu', @idabonnement, @iduser, NOW())";

                        using (var command = new MySqlCommand(insertquery, connection))
                        {
                            command.Parameters.AddWithValue("@message", message);
                            command.Parameters.AddWithValue("@type", type); // ou autre type selon votre logique
                            command.Parameters.AddWithValue("@idabonnement", abonnementId);
                            command.Parameters.AddWithValue("@iduser", userId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Gestion spécifique des erreurs MySQL
                    Console.WriteLine($"Erreur MySQL : {ex.Message}");
                    throw new Exception("Erreur lors de l'insertion de la notification dans la base de données.", ex);
                }
                catch (Exception ex)
                {
                    // Gestion des autres erreurs
                    Console.WriteLine($"Erreur générale : {ex.Message}");
                    throw new Exception("Une erreur s'est produite lors de la création de la notification.", ex);
                }
            }


        public List<Notification> GetNotificationsForClient()
        {
            List<Notification> notifications = new List<Notification>();
            int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            n.notification_id,  -- Si l'identifiant est 'notification_id' au lieu de 'id'
                            n.message, 
                            n.type, 
                            n.status, 
                            n.iduser, 
                            n.created_at, 
                            n.idabonnement,
                            a.nom AS abonnements, 
                            u.username AS user
                        FROM notifications n
                        JOIN abonnements a ON n.idabonnement = a.abonnement_id
                        JOIN users u ON n.iduser = u.id
                        WHERE u.id = @userId AND u.role = 'user'";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Notification notification = new Notification();
                                notification.Id = reader.GetInt32("notification_id");  // Assurez-vous de correspondre au bon nom de colonne
                                notification.Message = reader.GetString("message");
                                notification.Type = reader.GetString("type");
                                notification.Status = reader.GetString("status");
                                notification.UserId = reader.GetInt32("iduser");
                                notification.CreatedAt = reader.GetDateTime("created_at");
                                notification.AbonnementId = reader.GetInt32("idabonnement");

                                notifications.Add(notification);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return notifications;
        }



        public List<Notification> GetNotificationsForAdmin()
        {
            List<Notification> notifications = new List<Notification>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            n.notification_id,  -- Si l'identifiant est 'notification_id' au lieu de 'id'
                            n.message, 
                            n.type, 
                            n.status, 
                            n.iduser, 
                            n.created_at, 
                            n.idabonnement,
                            a.nom AS abonnements, 
                            u.username AS user
                        FROM notifications n
                        JOIN abonnements a ON n.idabonnement = a.abonnement_id
                        JOIN users u ON n.iduser = u.id
                        WHERE u.role = 'admin' 
                        ORDER BY N.status DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Notification notification = new Notification();
                                notification.Id = reader.GetInt32("notification_id");  // Assurez-vous de correspondre au nom de la colonne
                                notification.Message = reader.GetString("message");
                                notification.Type = reader.GetString("type");
                                notification.Status = reader.GetString("status");
                                notification.UserId = reader.GetInt32("iduser");
                                notification.CreatedAt = reader.GetDateTime("created_at");
                                notification.AbonnementId = reader.GetInt32("idabonnement");

                                notifications.Add(notification);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return notifications;
        }

        
        public void MarkNotificationAsRead(int notificationId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE notifications SET status = 'lu' WHERE notification_id = @notification_id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@notification_id", notificationId);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} rows updated");
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }



    }
}