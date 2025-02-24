namespace suivi_abonnement.Repository.Interface
{
    public interface INotifyEmailRepository 
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}