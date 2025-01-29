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
                        SendNotificationByRole("admin");
                    }
                    else
                    {
                        SendNotificationByRole( "user");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void SendNotificationByRole(string role)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT 
                                        a.abonnement_id,  -- Changer 'a.id' par 'a.abonnement_id'
                                        a.nom,
                                        u.id AS iduser,
                                        u.username,
                                        u.email
                                    FROM abonnements a
                                    JOIN users u ON u.role = @role
                                    WHERE a.expiration_date BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 1 MONTH)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@role", role);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("iduser");
                                int abonnementId = reader.GetInt32("abonnement_id");  // Assurez-vous d'utiliser le bon nom de colonne
                                string message = $"Cher {reader.GetString("username")}, l'abonnement {reader.GetString("nom")} va expirer dans un mois";

                                try
                                {

                                    CreateNotification(userId, abonnementId, message);
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
            catch (System.Exception)
            {
                throw;
            }
        }



        public void CreateNotification(int userId , int abonnementId , string message)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string checkAbonnementIsExist = "SELECT COUNT(*) FROM notifications WHERE idabonnement = @idabonnement AND iduser = @iduser";

                    using (var checkcommand = new MySqlCommand(checkAbonnementIsExist, connection))
                    {
                        checkcommand.Parameters.AddWithValue("@idabonnement", abonnementId);
                        checkcommand.Parameters.AddWithValue("@iduser", userId);

                        int AbonnementexistingToNotify = Convert.ToInt32(checkcommand.ExecuteScalar());

                        if (AbonnementexistingToNotify > 0)
                        {
                            return;
                        }
                    }

                    string insertquery = "INSERT INTO notifications (message , type , status , idabonnement , iduser , created_at) VALUES (@message , 'abonnement expirer' , 'non lu' , @idabonnement , @iduser , NOW())";

                    using (var command = new MySqlCommand(insertquery, connection))
                    {
                        command.Parameters.AddWithValue("@message", message);
                        command.Parameters.AddWithValue("@idabonnement", abonnementId);
                        command.Parameters.AddWithValue("@iduser", userId);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                
                throw new Exception(e.Message);
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
                        WHERE u.id = @userId AND u.role = 'user' AND n.status = 'non lu'";

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
                        WHERE u.role = 'admin' AND n.status = 'non lu'";

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