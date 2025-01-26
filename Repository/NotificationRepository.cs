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

        public NotificationRepository()
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public void SendNotification()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    
                    SendAdminNotification();
                    SendClientNotification();
                    

                    connection.Close();

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);     
            }
        }

        public void SendAdminNotification()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                            a.*,
                                            u.id AS iduser,
                                            u.username,
                                            u.email
                                    FROM
                                            abonnements a
                                    JOIN 
                                            users u ON u.role = 'admin'
                                    WHERE 
                                            a.expiration_date BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 1 MONTH)";
                    
                    using (var command = new MySqlCommand(query , connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("iduser");
                                int abonnementId = reader.GetInt32("id");
                                string message = "Cher Mr/Mme/Mlle " + reader.GetString("username") + " l'abonnement " + reader.GetString("nom") + " va expirer dans un mois";

                                CreateNotification(userId , abonnementId , message);
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
        }

        public void SendClientNotification()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                            a.*, 
                                            u.id AS iduser, 
                                            u.username, 
                                            u.email
                                        FROM
                                            abonnements a
                                        JOIN 
                                            departements d ON a.departement_id = d.departement_id
                                        JOIN
                                            departement_user du ON du.iddepartement = d.departement_id
                                        JOIN
                                            users u ON u.id = du.user_id
                                        WHERE 
                                            du.user_id = u.id
                                        AND
                                            a.expiration_date BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 1 MONTH)";

                    
                    using (var command = new MySqlCommand(query , connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("iduser");
                                int abonnementId = reader.GetInt32("id");
                                string message = "Cher Mr/Mme/Mlle " + reader.GetString("username") + " l'abonnement " + reader.GetString("nom") + " va expirer dans un mois";

                                CreateNotification(userId , abonnementId , message);
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
        }

        public void CreateNotification(int userId , int abonnementId , string message)
        {
            Notification notificationobj = new Notification();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO notifications (message , type , status , idabonnement , iduser) VALUES (@message , 'abonnement expirer' , 'non lu' , @idabonnement , @iduser)";

                    using (var command = new MySqlCommand(query, connection))
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

        public List<Notification> GetNotificationsForClient(int userId)
        {
            List<Notification> notifications = new List<Notification>();
            try
            {

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT n* , a.nom AS abonnement , u.username AS user FROM notifications n JOIN abonnement a ON n.idabonnement = a.id JOIN user u ON n.iduser = u.id WHERE u.id = @iduser AND n.statut = 'non lu' AND u.role = 'user'";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@iduser", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Notification notification = new Notification();
                                notification.Id = reader.GetInt32("id");
                                notification.Message = reader.GetString("message");
                                notification.Type = reader.GetString("type");
                                notification.Status = reader.GetString("status");
                                notification.UserId = reader.GetInt32("iduser");
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
                    string query = "SELECT n* , a.nom AS abonnement , u.username AS user FROM notifications n JOIN abonnement a ON n.idabonnement = a.id JOIN user u ON n.iduser = u.id WHERE u.role = 'admin' AND n.status = 'non lu'";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Notification notification = new Notification();
                                notification.Id = reader.GetInt32("id");
                                notification.Message = reader.GetString("message");
                                notification.Type = reader.GetString("type");
                                notification.Status = reader.GetString("status");
                                notification.UserId = reader.GetInt32("iduser");
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

    }
}