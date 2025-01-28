using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Service.Interface;
using System;

namespace suivi_abonnement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        // Injection de dépendance pour accéder à INotificationService
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Action pour marquer une notification comme lue
        

    }
}
