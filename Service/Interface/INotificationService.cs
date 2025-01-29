using suivi_abonnement.Models;
namespace suivi_abonnement.Service.Interface
{
    public interface INotificationService
    {
        void SendNotification();
        void SendNotificationByRole(string role);
        void CreateNotification(int userId , int abonnementId , string message);
        List<Notification> GetNotificationsForAdmin();
        List<Notification> GetNotificationsForClient();
        void MarkNotificationAsRead(int notificationId);
        
    }

    
}