using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using suivi_abonnement.Models;
using Microsoft.AspNetCore.Http;
using suivi_abonnement.Service;
using suivi_abonnement.Service.Interface; // Assurez-vous d'ajouter le bon espace de noms pour _notificationService

namespace suivi_abonnement.Controllers
{
    public class BaseController : Controller
    {
        private readonly INotificationService _notificationService;

        public BaseController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var userRole = HttpContext.Session.GetString("UserRole");

            
            _notificationService.SendNotification();

            List<Notification> notifications = new List<Notification>();

            if (userRole == "admin")
            {
                notifications = _notificationService.GetNotificationsForAdmin();

            }
            else if (userRole == "client")
            {
                notifications = _notificationService.GetNotificationsForClient();

            }
            else
            {
                Console.WriteLine("❌ Rôle utilisateur inconnu.");
            }

            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;
            

            ViewBag.Notifications = notifications;
            ViewBag.NbrNotifications = notificationCount;

            base.OnActionExecuting(context);
        }
    }
}
