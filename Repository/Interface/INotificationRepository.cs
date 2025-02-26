using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface INotificationRepository
    {
        void SendNotification();
        void SendNotificationByRoleAdmin();
        void SendNotificationByRoleUser();
        void CreateNotification(int userId , int abonnementId , string message , string type);
        List<Notification> GetNotificationsForAdmin();
        List<Notification> GetNotificationsForClient();
        Notification MarkNotificationAsRead(int notificationId);
        List<Notification> GetUnreadNotifications(int userId);
        
    }
}