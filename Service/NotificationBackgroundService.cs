namespace suivi_abonnement.Service
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly NotificationService _notificationService;

        public NotificationBackgroundService(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _notificationService.SendNotification();
                await Task.Delay(1000 * 60 * 60 * 24, stoppingToken);
            }
        }
    }
}