using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface INotificationRepository
    {
        void SendNotification();
        void SendAdminNotification();
        void SendClientNotification();
        void CreateNotification(int userId , int abonnementId , string message);
        List<Notification> GetNotificationsForAdmin();
        List<Notification> GetNotificationsForClient(int userId);
        
    }
}