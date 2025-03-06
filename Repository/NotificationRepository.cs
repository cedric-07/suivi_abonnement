using suivi_abonnement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
using suivi_abonnement.Hubs;
using Microsoft.AspNetCore.SignalR;
namespace suivi_abonnement.Service
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly string connectionString;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly INotifyEmailRepository _notifyEmailRepository;
        private readonly IHubContext<NotificationHub> _hubContext;  
        public NotificationRepository(IHttpContextAccessor httpContextAccessor , IServiceProvider serviceProvider , IHubContext<NotificationHub> hubContext, INotifyEmailRepository notifyEmailRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _notifyEmailRepository = notifyEmailRepository;
            _hubContext = hubContext;
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public void SendNotification()
        {
            var userRole = _httpContextAccessor.HttpContext?.Session?.GetString("UserRole");

            if (userRole == null)
            {
                throw new Exception("UserRole is not set in the session.");
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

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
                throw new Exception(e.Message);
            }
        }



        //public void SendNotificationByRoleAdmin()
        //{
        //    try
        //    {
        //        using (var connection = new MySqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            string query = @"
        //                            SELECT 
        //                                a.abonnement_id, 
        //                                a.nom, 
        //                                u.id AS iduser, 
        //                                u.username, 
        //                                u.email, 
        //                                DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants 
        //                            FROM abonnements a
        //                            JOIN users u ON u.role = 'admin'  -- Suppression de 'u.id = a.user_id'
        //                            WHERE a.expiration_date >= CURDATE()";
        //            using (var command = new MySqlCommand(query, connection))
        //            {
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        int userId = reader.GetInt32("iduser");
        //                        int abonnementId = reader.GetInt32("abonnement_id");
        //                        int joursRestants = reader.GetInt32("jours_restants");
        //                        string nomAbonnement = reader.GetString("nom");
        //                        string nomClient = reader.GetString("username");
        //                        string emailClient = reader.GetString("email");

        //                        if (joursRestants >= 15 && joursRestants <= 30)
        //                        {
        //                            string type = "Rappel";
        //                            string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                            CreateNotification(userId, abonnementId, message, type);
        //                        }
        //                        else if (joursRestants <= 7)
        //                        {
        //                            string type = "Alerte";
        //                            string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                            CreateNotification(userId, abonnementId, message, type);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Erreur lors de l'envoi des notifications Admin : " + e.Message);
        //    }
        //}

        // public void SendNotificationByRoleAdmin()
        // {
        //     try
        //     {
        //         using (var connection = new MySqlConnection(connectionString))
        //         {
        //             connection.Open();
        //             // string query = @"
        //             //         SELECT 
        //             //             a.abonnement_id, 
        //             //             a.nom, 
        //             //             u.id AS iduser, 
        //             //             u.username, 
        //             //             u.email, 
        //             //             DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants 
        //             //         FROM abonnements a
        //             //         JOIN users u ON u.role = 'admin'  
        //             //         WHERE a.expiration_date >= CURDATE()";

        
        //             string query = @"
        //                             SELECT 
        //                                 a.abonnement_id, 
        //                                 a.nom, 
        //                                 u.id AS iduser, 
        //                                 u.username, 
        //                                 u.email, 
        //                                 DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants 
        //                             FROM abonnements a
        //                             JOIN users u ON LOWER(u.role) = 'admin'  
        //                             WHERE a.expiration_date >= CURDATE()
        //                             AND a.expiration_date >= DATE_FORMAT(CURDATE(), '%Y-%m-01')";


        //             using (var command = new MySqlCommand(query, connection))
        //             {
        //                 using (var reader = command.ExecuteReader())
        //                 {
        //                     while (reader.Read())
        //                     {
        //                         int userId = reader.GetInt32("iduser");
        //                         int abonnementId = reader.GetInt32("abonnement_id");
        //                         int joursRestants = reader.GetInt32("jours_restants");
        //                         string nomAbonnement = reader.GetString("nom");

        //                         if (joursRestants >= 15 && joursRestants <= 30)
        //                         {
        //                             string type = "Rappel";
        //                             string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                             CreateNotification(userId, abonnementId, message, type, joursRestants);
        //                         }
        //                         else if (joursRestants <= 7)
        //                         {
        //                             string type = "Alerte";
        //                             string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                             CreateNotification(userId, abonnementId, message, type, joursRestants);
        //                         }
        //                         else if (joursRestants == 0)
        //                         {
        //                             string type = "Alerte";
        //                             string message = type + " : L'abonnement " + nomAbonnement + " expire aujourd'hui";
        //                             CreateNotification(userId, abonnementId, message, type, joursRestants);
        //                         }
                                
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         throw new Exception("Erreur lors de l'envoi des notifications Admin : " + e.Message);
        //     }
        // }
        public void SendNotificationByRoleAdmin()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            a.abonnement_id, 
                            a.nom, 
                            u.id AS iduser, 
                            u.username, 
                            u.email, 
                            DATEDIFF(a.expiration_date, CURDATE()) AS jours_restants 
                        FROM abonnements a
                        JOIN users u ON LOWER(u.role) = 'admin'  
                        ORDER BY a.expiration_date";

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

                                // 🔵 Expiration Proche (15 à 30 jours)
                                if (joursRestants >= 15 && joursRestants <= 30)
                                {
                                    string type = "Rappel";
                                    string message = $"{type} : {nomAbonnement} expire dans {joursRestants} jours.";
                                    CreateNotification(userId, abonnementId, message, type , joursRestants);
                                }
                                // 🟠 Expiration Très Bientôt (7 jours ou moins)
                                else if (joursRestants <= 7 && joursRestants > 0)
                                {
                                    string type = "Alerte";
                                    string message = $"{type} : {nomAbonnement} expire dans {joursRestants} jours.";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
                                }
                                // 🔴 Expire Aujourd’hui
                                else if (joursRestants == 0)
                                {
                                    string type = "Urgent";
                                    string message = $"{type} : {nomAbonnement} expire aujourd'hui !";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
                                }
                                // ❌ Déjà Expiré
                                else if (joursRestants < 0)
                                {
                                    string type = "Expiration";
                                    string message = $"{type} : {nomAbonnement} est expiré depuis {Math.Abs(joursRestants)} jours.";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erreur lors de l'envoi des notifications Admin : " + e.Message);
            }
        }




        //public void SendNotificationByRoleUser()
        //{
        //    try
        //    {
        //        using (var connection = new MySqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            string query = @"SELECT idclient , abonnement_id , nomabonnement , nomclient , emailclient , idclient , DATEDIFF(expiration_date, CURDATE()) AS jours_restants FROM v_abonnements_par_client WHERE roleclient = 'user' AND expiration_date >= CURDATE()";

        //            using (var command = new MySqlCommand(query, connection))
        //            {
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        int userId = reader.GetInt32("idclient");
        //                        int abonnementId = reader.GetInt32("abonnement_id");
        //                        int joursRestants = reader.GetInt32("jours_restants");
        //                        string nomAbonnement = reader.GetString("nomabonnement");
        //                        string nomClient = reader.GetString("nomclient");
        //                        string emailClient = reader.GetString("emailclient");

        //                        if (joursRestants >= 15 && joursRestants <= 30)
        //                        {
        //                            string type = "Rappel";
        //                            string message = type + " : Votre abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                            CreateNotification(userId, abonnementId, message, type);
        //                        }
        //                        else if (joursRestants <= 7)
        //                        {
        //                            string type = "Alerte";
        //                            string message = type + " : Votre abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
        //                            CreateNotification(userId, abonnementId, message, type);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Erreur lors de l'envoi des notifications User : " + e.Message);
        //    }
        //}

        public void SendNotificationByRoleUser()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // string query = @"SELECT idclient , abonnement_id , nomabonnement , nomclient , emailclient , idclient , DATEDIFF(expiration_date, CURDATE()) AS jours_restants FROM v_abonnements_par_client WHERE roleclient = 'user' AND expiration_date >= CURDATE()";
                    // string query = @"
                    //                 SELECT idclient, abonnement_id, nomabonnement, nomclient, emailclient, idclient, 
                    //                     DATEDIFF(expiration_date, CURDATE()) AS jours_restants 
                    //                 FROM v_abonnements_par_client 
                    //                 WHERE roleclient = 'user' 
                    //                 AND expiration_date >= CURDATE()
                    //                 AND expiration_date >= DATE_FORMAT(CURDATE(), '%Y-%m-01')";
                    string query = @"SELECT idclient , abonnement_id , nomabonnement , nomclient , emailclient , idclient , DATEDIFF(expiration_date, CURDATE()) AS jours_restants FROM v_abonnements_par_client WHERE roleclient = 'user' ORDER BY expiration_date";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("idclient");
                                int abonnementId = reader.GetInt32("abonnement_id");
                                int joursRestants = reader.GetInt32("jours_restants");
                                string nomAbonnement = reader.GetString("nomabonnement");

                                if (joursRestants >= 15 && joursRestants <= 30)
                                {
                                    string type = "Rappel";
                                    string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
                                }
                                else if (joursRestants <= 7)
                                {
                                    string type = "Alerte";
                                    string message = type + " : L'abonnement " + nomAbonnement + " va expirer dans " + joursRestants + " jours";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
                                }
                                else if(joursRestants == 0)
                                {
                                    string type = "Alerte";
                                    string message = type + " : L'abonnement " + nomAbonnement + " expire aujourd'hui";
                                    CreateNotification(userId, abonnementId, message, type, joursRestants);
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

        //public void CreateNotification(int userId, int abonnementId, string message, string type)
        //{
        //    try
        //    {
        //        using (var connection = new MySqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            string checkAbonnementIsExist = "SELECT COUNT(*) FROM notifications WHERE idabonnement = @idabonnement AND iduser = @iduser";
        //            using (var checkcommand = new MySqlCommand(checkAbonnementIsExist, connection))
        //            {
        //                checkcommand.Parameters.AddWithValue("@idabonnement", abonnementId);
        //                checkcommand.Parameters.AddWithValue("@iduser", userId);

        //                int existingCount = Convert.ToInt32(checkcommand.ExecuteScalar());

        //                if (existingCount > 0)
        //                {
        //                    return;
        //                }
        //            }

        //            string insertquery = "INSERT INTO notifications (message, type, status, idabonnement, iduser, created_at) " +
        //                                "VALUES (@message, @type, 'non lu', @idabonnement, @iduser, NOW())";

        //            using (var command = new MySqlCommand(insertquery, connection))
        //            {
        //                command.Parameters.AddWithValue("@message", message);
        //                command.Parameters.AddWithValue("@type", type);
        //                command.Parameters.AddWithValue("@idabonnement", abonnementId);
        //                command.Parameters.AddWithValue("@iduser", userId);
        //                command.ExecuteNonQuery();
        //            }
        //        }

        //        // 🔥 Envoyer la notification en temps réel après l'insertion dans la base de données
        //        var notificationCount = GetUnreadNotificationCount(userId);
        //        _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message, notificationCount);
        //        Console.WriteLine($"📨 Notification en temps réel envoyée à {userId}");



        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Erreur lors de la création de la notification : {ex.Message}");
        //    }
        //}

        public void CreateNotification(int userId, int abonnementId, string message, string type, int joursRestants)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Vérifier si une notification existe déjà aujourd'hui pour cet abonnement
                    string checkNotificationExist = "SELECT COUNT(*) FROM notifications WHERE idabonnement = @idabonnement AND iduser = @iduser AND DATE(created_at) = CURDATE()";

                    using (var checkCommand = new MySqlCommand(checkNotificationExist, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                        checkCommand.Parameters.AddWithValue("@iduser", userId);

                        int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            // 🔄 Mettre à jour la notification existante pour refléter le nouveau nombre de jours restants
                            string updateQuery = "UPDATE notifications SET message = @message, created_at = NOW() WHERE idabonnement = @idabonnement AND iduser = @iduser AND DATE(created_at) = CURDATE()";

                            using (var updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@message", message);
                                updateCommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                                updateCommand.Parameters.AddWithValue("@iduser", userId);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // 🆕 Insérer une nouvelle notification
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

                // 🔥 Envoyer la notification en temps réel
                var notificationCount = GetUnreadNotificationCount(userId);
                _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message, notificationCount);
                Console.WriteLine($"📨 Notification mise à jour/enregistrée pour {userId} : {message}");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int GetUnreadNotificationCount(int userId)
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM notifications WHERE iduser = @userId AND status = 'non lu'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
        
        public List<Notification> GetUnreadNotifications(int userId)
        {
            List<Notification> notifications = new List<Notification>();
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
            }
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM notifications WHERE iduser = @userId AND status = 'non lu'";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Notification notification = new Notification
                                {
                                    Id = reader.GetInt32("notification_id"),
                                    Message = reader.GetString("message"),
                                    Type = reader.GetString("type"),
                                    Status = reader.GetString("status"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    
                                };
                                notifications.Add(notification);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return notifications;

        }

        public List<Notification> GetNotificationsForClient()
        {
            List<Notification> notifications = new List<Notification>();
            int userId = 0;
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
            }
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

        
        public Notification MarkNotificationAsRead(int notificationId)
        {
            Notification notification = new Notification();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE notifications SET status = 'lu' WHERE notification_id = @notificationId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@notificationId", notificationId);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return notification;
        }

        //protected async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var notificationRepo = scope.ServiceProvider.GetRequiredService<NotificationRepository>();
        //        }

        //        // Attendre 24h avant la prochaine exécution
        //        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        //    }
        //}

        
        protected  async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationRepo = scope.ServiceProvider.GetRequiredService<NotificationRepository>();

                    try
                    {
                        Console.WriteLine("🔔 Exécution de la vérification des abonnements...");

                        // Appel de la méthode qui envoie les notifications
                        notificationRepo.SendNotification();

                        Console.WriteLine("✅ Notifications envoyées !");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur lors de l'envoi des notifications : {ex.Message}");
                    }
                }

                // Attendre 24 heures avant la prochaine exécution
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

    }
}