using System;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly HttpContextAccessor _httpContextAccessor;
        public NotificationController(INotificationService notificationService, HttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }
        
        public IActionResult IndexNotification()
        {
            var userRole = HttpContext.Session.GetString("userRole");
            int userId = HttpContext.Session.GetInt32("userId") ?? 0;
            if (string.IsNullOrEmpty(userRole) || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }
            
            _notificationService.SendNotification();
            List<Notification> notifications = new List<Notification>();
            if (userRole == "Admin")
            {
                notifications = _notificationService.GetNotificationsForAdmin();
            }

            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;

            ViewBag.NotificationCount = notificationCount;

            return View("~/Views/shared/NavbarAdmin/_NavbarLayout.cshtml" , notifications);
        }
    }
}