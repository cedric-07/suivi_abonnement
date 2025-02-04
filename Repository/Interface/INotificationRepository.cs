using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface INotificationRepository
    {
        void SendNotification();
        void SendNotificationByRoleAdmin(string role);
        void SendNotificationByRoleUser(string role);
        void CreateNotification(int userId , int abonnementId , string message , string type);
        List<Notification> GetNotificationsForAdmin();
        List<Notification> GetNotificationsForClient();
        void MarkNotificationAsRead(int notificationId);
        
    }
}