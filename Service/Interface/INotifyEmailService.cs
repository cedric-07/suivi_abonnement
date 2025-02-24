namespace suivi_abonnement.Service.Interface
{
    public interface INotifyEmailService
    {
        Task SendEmailAsync(string toemail, string subject, string message);
    }
}