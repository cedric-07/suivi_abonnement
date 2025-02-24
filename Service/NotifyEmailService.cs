using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
using System.Threading.Tasks;
namespace suivi_abonnement.Service
{
    public class NotifyEmailService : INotifyEmailService
    {
        private readonly INotifyEmailRepository _notifyEmailRepository;
        public NotifyEmailService(INotifyEmailRepository notifyEmailRepository)
        {
            _notifyEmailRepository = notifyEmailRepository;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            await _notifyEmailRepository.SendEmailAsync(toEmail, subject, message);
        }
    }
}