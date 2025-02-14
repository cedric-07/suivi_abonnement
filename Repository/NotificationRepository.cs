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
        private readonly IServiceProvider _serviceProvider;

        public NotificationRepository(IHttpContextAccessor httpContextAccessor , IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
            _serviceProvider = serviceProvider;
        }

        public void SendNotification()
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 🔥 Suppression des notifications du jour précédent
                    DeleteOldNotifications();

                    if (userRole == "admin")
                    {
                        SendNotificationByRoleAdmin();
                    }
                    else if (userRole == "user")
                    {
                        SendNotificationByRoleUser();
                    }
                    else
                    {
                        throw new Exception("Role non reconnu");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de l'envoi des notifications : " + e.Message);
            }
        }


        /// <summary>
        /// Envoi de notifications pour les administrateurs
        /// </summary>
        public void SendNotificationByRoleAdmin()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    int[] joursNotification = {30, 15, 7, 3, 1};

                    string query = @"
                                    SELECT 
                                        a.abonnement_id, 
                                        a.nom, 
                                        u.id AS iduser, 
                                        u.username, 
                                        u.email, 
                                        DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants 
                                    FROM abonnements a
                                    JOIN users u ON u.role = 'admin'  -- Suppression de 'u.id = a.user_id'
                                    WHERE a.expiration_date >= CURDATE() 
                                    AND DATEDIFF(a.expiration_date, CURDATE()) IN (30, 15, 7, 3, 1)";


                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("iduser");
                                int abonnementId = reader.GetInt32("abonnement_id");
                                int joursRestants = reader.GetInt32("jours_restants");
                                string nomAbonnement = reader.GetString("nom");
                                string nomClient = reader.GetString("username");
                                string emailClient = reader.GetString("email");

                                if (Array.Exists(joursNotification, jour => jour == joursRestants))
                                {
                                    string message = GetDailyNotificationMessage(joursRestants, nomClient, nomAbonnement, "admin");
                                    string type = (joursRestants <= 7) ? "Rappel " : "Alerte";

                                    try
                                    {
                                        CreateNotification(userId, abonnementId, message, type);
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
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de l'envoi des notifications Admin : " + e.Message);
            }
        }

        /// <summary>
        /// Envoi de notifications pour les utilisateurs
        /// </summary>
        public void SendNotificationByRoleUser()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT idclient, abonnement_id, nomabonnement, nomclient, emailclient, expiration_date
                        FROM v_abonnements_par_client 
                        WHERE roleclient = 'user'
                        AND expiration_date >= CURDATE()
                        AND expiration_date <= DATE_ADD(CURDATE(), INTERVAL 30 DAY)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("idclient");
                                int abonnementId = reader.GetInt32("abonnement_id");
                                string nomAbonnement = reader.GetString("nomabonnement");
                                string nomClient = reader.GetString("nomclient");
                                string emailClient = reader.GetString("emailclient");
                                DateTime expirationDate = reader.GetDateTime("expiration_date");

                                int joursRestants = CalculerJoursOuvres(DateTime.Today, expirationDate);

                                if (joursRestants >= 0)  // Exclure les abonnements déjà expirés
                                {
                                    string message = GetDailyNotificationMessage(joursRestants, nomClient, nomAbonnement, "user");
                                    string type = (joursRestants <= 7) ? "Rappel" : "Alerte";

                                    try
                                    {
                                        CreateNotification(userId, abonnementId, message, type);
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
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de l'envoi des notifications User : " + e.Message);
            }
        }

        /// <summary>
        /// Crée une notification en base de données
        /// </summary>
        public void CreateNotification(int userId, int abonnementId, string message, string type)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM notifications WHERE idabonnement = @idabonnement AND iduser = @iduser AND DATE(created_at) = CURDATE()";

                    using (var checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                        checkCommand.Parameters.AddWithValue("@iduser", userId);

                        int existingNotifications = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (existingNotifications > 0)
                        {
                            // Mise à jour de la notification existante
                            string updateQuery = "UPDATE notifications SET message = @message, type = @type, created_at = NOW() WHERE idabonnement = @idabonnement AND iduser = @iduser AND DATE(created_at) = CURDATE() AND status = 'non lu'";


                            using (var updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@message", message);
                                updateCommand.Parameters.AddWithValue("@type", type);
                                updateCommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                                updateCommand.Parameters.AddWithValue("@iduser", userId);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insertion d'une nouvelle notification
                            string insertQuery = "INSERT INTO notifications (message, type, status, idabonnement, iduser, created_at) " +
                                                "VALUES (@message, @type, 'non lu', @idabonnement, @iduser, NOW())";

                            using (var insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@message", message);
                                insertCommand.Parameters.AddWithValue("@type", type);
                                insertCommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                                insertCommand.Parameters.AddWithValue("@iduser", userId);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                throw new Exception("Erreur lors de la mise à jour ou création de la notification.", ex);
            }
        }


        /// <summary>
        /// Calcul du nombre de jours ouvrés entre deux dates (exclut samedi et dimanche)
        /// </summary>
        private int CalculerJoursOuvres(DateTime startDate, DateTime endDate)
        {
            int joursOuvres = 0;

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    joursOuvres++;
                }
            }

            return joursOuvres;
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

        private string GetDailyNotificationMessage(int joursRestants, string nomClient, string nomAbonnement, string role)
        {
            string[] messagesAdmin = {
                $"Alerte : L'abonnement '{nomAbonnement}' de {nomClient} expire bientôt !",
                $"Alerte : l'abonnement '{nomAbonnement}' expire dans {joursRestants} jour(s). Veillez à informer les utilisateurs.",
                $"Rappel : L'abonnement '{nomAbonnement}' expire sous peu. Assurez-vous du renouvellement.",
                $"Rappel : L'abonnement '{nomAbonnement}' arrive à expiration dans {joursRestants} jours.",
                $"Rappel : Veillez à surveiller l'expiration de '{nomAbonnement}' pour l'utilisateur de cet abonnement."
            };

            string[] messagesUser = {
                $"Bonjour {nomClient}, votre abonnement '{nomAbonnement}' expire bientôt !",
                $"Attention {nomClient}, il ne vous reste que {joursRestants} jour(s) pour renouveler '{nomAbonnement}'.",
                $"Rappel : L'abonnement '{nomAbonnement}' expire sous peu. Pensez à le renouveler.",
                $"Notification quotidienne : votre abonnement '{nomAbonnement}' prend fin dans {joursRestants} jours.",
                $"{nomClient}, assurez-vous que '{nomAbonnement}' soit renouvelé avant expiration."
            };

            // Sélectionne un message aléatoire différent pour Admin et User
            int index = DateTime.Now.Day % messagesAdmin.Length;  
            return role == "admin" ? messagesAdmin[index] : messagesUser[index];
        }


        public void UpdateDailyNotifications()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT n.notification_id, n.iduser, n.idabonnement, a.nom AS nomAbonnement, 
                            u.username AS nomClient, u.role AS userRole,
                            DATEDIFF(a.expiration_date, CURDATE()) AS joursRestants
                        FROM notifications n
                        JOIN abonnements a ON n.idabonnement = a.abonnement_id
                        JOIN users u ON n.iduser = u.id
                        WHERE a.expiration_date >= CURDATE()";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int notificationId = reader.GetInt32("notification_id");
                                int userId = reader.GetInt32("iduser");
                                int abonnementId = reader.GetInt32("idabonnement");
                                string nomAbonnement = reader.GetString("nomAbonnement");
                                string nomClient = reader.GetString("nomClient");
                                string userRole = reader.GetString("userRole"); // Récupération du rôle
                                int joursRestants = reader.GetInt32("joursRestants");

                                // Génération d'un message spécifique à l'admin ou à l'utilisateur
                                string newMessage = GetDailyNotificationMessage(joursRestants, nomClient, nomAbonnement, userRole);

                                // Mise à jour du message de la notification
                                string updateQuery = "UPDATE notifications SET message = @message WHERE notification_id = @notification_id";

                                using (var updateCommand = new MySqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@message", newMessage);
                                    updateCommand.Parameters.AddWithValue("@notification_id", notificationId);
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la mise à jour des notifications : " + e.Message);
            }
        }
    

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationRepo = scope.ServiceProvider.GetRequiredService<NotificationRepository>();
                    notificationRepo.UpdateDailyNotifications();
                }

                // Attendre 24h avant la prochaine exécution
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        public void DeleteOldNotifications()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM notifications WHERE DATE(created_at) < CURDATE()";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} notifications supprimées.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la suppression des anciennes notifications : " + e.Message);
            }
        }


    }
}