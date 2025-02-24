using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Models;
using MySql.Data.MySqlClient;
using suivi_abonnement.Service.Interface;
public class NotificationHub : Hub
{
    // Stocke les utilisateurs connectés (userId, ConnectionId, userRole)
    private static ConcurrentDictionary<int, (string ConnectionId, string UserRole)> ConnectedUsers = new ConcurrentDictionary<int, (string, string)>();
    private readonly INotificationService _notificationService;
    private readonly string connectionString;
    private readonly INotifyEmailService _emailService;
    private readonly IUserService _userService;

    // 🔹 Injection de `INotificationService`
    public NotificationHub(INotificationService notificationService, INotifyEmailService emailService, IUserService userService)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _userService = userService;
        connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                Console.WriteLine("⚠️ HttpContext est null dans OnConnectedAsync.");
                return;
            }

            var session = httpContext.Session;
            int? userId = session.GetInt32("UserId");
            string userRole = session.GetString("UserRole") ?? "unknown";

            if (!userId.HasValue)
            {
                Console.WriteLine("⚠️ Impossible de récupérer UserId depuis la session.");
                return;
            }

            Console.WriteLine($"✅ Utilisateur connecté - ID: {userId}, Rôle: {userRole}, ConnectionId: {Context.ConnectionId}");

            // 🔄 Suppression de l'ancien `ConnectionId` (évite les conflits)
            ConnectedUsers.AddOrUpdate(userId.Value, (Context.ConnectionId, userRole), (key, oldValue) => (Context.ConnectionId, userRole));

            // 🚀 🔥 CORRECTION : Toujours récupérer les notifications depuis la base après mise à jour
            List<Notification> notifications = userRole == "admin"
                ? _notificationService.GetNotificationsForAdmin()
                : _notificationService.GetNotificationsForClient();

            int notificationCount = notifications.Count(n => n.Status == "non lu");

            // 📡 ENVOYER LES NOTIFICATIONS À JOUR
            await Clients.Client(Context.ConnectionId).SendAsync("LoadNotifications", notifications, notificationCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur dans OnConnectedAsync : {ex.Message}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var user = ConnectedUsers.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId);
            if (user.Key != 0)
            {
                Console.WriteLine($"❌ Déconnexion - Utilisateur ID: {user.Key}");
                ConnectedUsers.TryRemove(user.Key, out _);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur lors de la suppression de l'utilisateur déconnecté : {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotificationToUser(int userId, string message)
    {
        try
        {
            Console.WriteLine($"📩 Tentative d'envoi de notification à {userId}: {message}");

            if (!ConnectedUsers.TryGetValue(userId, out var userInfo))
            {
                Console.WriteLine($"⚠️ Utilisateur {userId} non trouvé dans ConnectedUsers !");
                return;
            }


            // 🔄 Charger uniquement les notifications non lues
            List<Notification> notifications = userInfo.UserRole == "admin"
                ? _notificationService.GetNotificationsForAdmin()
                : _notificationService.GetNotificationsForClient();

            int notificationCount = notifications.Count;

            // 📡 Envoyer la liste complète des notifications
            await Clients.Client(userInfo.ConnectionId).SendAsync("ReceiveNotification", notifications, notificationCount);
            Console.WriteLine($"📨 Notification envoyée à {userId} avec {notificationCount} notifications non lues !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de l'envoi de la notification à {userId} : {ex.Message}");
        }
    }

     public async Task SendEmailNotificationToUser(int userId, string subject, string message)
    {
        try
        {
            Console.WriteLine($"📩 Préparation de l'envoi d'un email à {userId}...");

            string userEmail = GetUserEmail(userId);

            if (string.IsNullOrEmpty(userEmail))
            {
                Console.WriteLine($"⚠️ Aucun email trouvé pour l'utilisateur {userId}");
                return;
            }

            await _emailService.SendEmailAsync(userEmail, subject, message);
            Console.WriteLine($"✅ Email envoyé avec succès à {userEmail}");

            // Envoyer un message à l'utilisateur connecté via SignalR
            if (ConnectedUsers.TryGetValue(userId, out var userInfo))
            {
                await Clients.Client(userInfo.ConnectionId).SendAsync("EmailSentNotification", "📧 Email envoyé avec succès !");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de l'envoi de l'email : {ex.Message}");
            if (ConnectedUsers.TryGetValue(userId, out var userInfo))
            {
                await Clients.Client(userInfo.ConnectionId).SendAsync("EmailSentNotification", "❌ Échec de l'envoi de l'email.");
            }
        }
    }


    // 🔹 Fonction pour récupérer l'email de l'utilisateur
    private string GetUserEmail(int userId)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT email FROM users WHERE id = @userId";
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                var result = command.ExecuteScalar();
                return result?.ToString();
            }
        }
    }


    // 🔹 Méthode pour marquer une notification comme lue
    public async Task MarkNotificationAsRead(int userId, int notificationId)
    {
        try
        {
            if (ConnectedUsers.TryGetValue(userId, out var userInfo))
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE notifications SET status = 'lu' WHERE notification_id = @id AND iduser = @userId";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", notificationId);
                        command.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} notifications marquées comme lues.");
                    }
                    connection.Close();
                }

                // 🔄 Charger la nouvelle liste APRES mise à jour
                List<Notification> notifications = userInfo.UserRole == "admin"
                    ? _notificationService.GetNotificationsForAdmin()
                    : _notificationService.GetNotificationsForClient();

                int notificationCount = notifications.Count(n => n.Status == "non lu");

                // 📡 Envoyer la mise à jour AU CLIENT
                await Clients.Client(userInfo.ConnectionId).SendAsync("UpdateNotifications", notifications, notificationCount);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ Erreur lors du marquage comme lu : {e.Message}");
        }
    }
}
