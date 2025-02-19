using suivi_abonnement.Models;
namespace suivi_abonnement.Service.Interface
{
    public interface INotificationService
    {
        void SendNotification();
        void SendNotificationByRoleAdmin();
        void SendNotificationByRoleUser();
        void CreateNotification(int userId , int abonnementId , string message , string type);
        List<Notification> GetNotificationsForAdmin();
        List<Notification> GetNotificationsForClient();
        Notification MarkNotificationAsRead(int notificationId);
        
    }

    
}