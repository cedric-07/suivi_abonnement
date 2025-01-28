using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Repository.Interface;
namespace suivi_abonnement.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public void SendNotification()
        {
            _notificationRepository.SendNotification();
        }
        public void SendNotificationByRole(string role)
        {
            _notificationRepository.SendNotificationByRole(role);
        }
        public void CreateNotification(int userId, int abonnementId, string message)
        {
             _notificationRepository.CreateNotification(userId, abonnementId, message);
        }
        public List<Notification> GetNotificationsForAdmin()
        {
            return _notificationRepository.GetNotificationsForAdmin();
        }
        public List<Notification> GetNotificationsForClient(int userId)
        {
            return _notificationRepository.GetNotificationsForClient(userId);
        }   
        public void MarkNotificationAsRead(int notificationId)
        {
            _notificationRepository.MarkNotificationAsRead(notificationId);
        }
    }
}