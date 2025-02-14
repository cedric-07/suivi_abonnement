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
        public void SendNotificationByRoleAdmin()
        {
            _notificationRepository.SendNotificationByRoleAdmin();
        }
        public void SendNotificationByRoleUser()
        {
            _notificationRepository.SendNotificationByRoleUser();
        }
        public void CreateNotification(int userId, int abonnementId, string message , string type)
        {
             _notificationRepository.CreateNotification(userId, abonnementId, message , type);
        }
        public List<Notification> GetNotificationsForAdmin()
        {
            return _notificationRepository.GetNotificationsForAdmin();
        }
        public List<Notification> GetNotificationsForClient()
        {
            return _notificationRepository.GetNotificationsForClient();
        }   
        public void MarkNotificationAsRead(int notificationId)
        {
            _notificationRepository.MarkNotificationAsRead(notificationId);
        }
    }
}