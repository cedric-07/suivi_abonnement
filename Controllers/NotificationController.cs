    using Microsoft.AspNetCore.Mvc;
    using suivi_abonnement.Service.Interface;
    using System;
    using suivi_abonnement.Models;
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

            public IActionResult Index()
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userRole) || userId == 0)
                {
                    throw new Exception("Vous n'êtes pas connecté");
                }

                _notificationService.SendNotification();

                var notifications = userRole == "admin" ? _notificationService.GetNotificationsForAdmin() : _notificationService.GetNotificationsForClient();

                // Vérification des notifications avant de les passer à la vue
                if (notifications == null || !notifications.Any())
                {
                    ViewBag.NbrNotifications = 0;
                    ViewBag.Notifications = new List<Notification>();  // Assurez-vous que la liste est vide
                }
                else
                {
                    ViewBag.NbrNotifications = notifications.Count;
                    ViewBag.Notifications = notifications;
                }

                return userRole == "admin" ? View("~/Views/Shared/Navbar/NavbarAdmin.cshtml") : View("~/Views/Shared/Navbar/NavbarUser.cshtml");
            }

            public JsonResult GetNotificationsCount()
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                var userRole = HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userRole) || userId == 0)
                {
                    throw new Exception("Vous n'êtes pas connecté");
                }


                var notifications = userRole == "admin" ? _notificationService.GetNotificationsForAdmin() : _notificationService.GetNotificationsForClient();

                int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;

                // Vérification des notifications avant de les passer à la vue
                if (notifications == null || !notifications.Any())
                {
                    return Json(new {notifications , notificationCount});
                }
                else
                {
                    return Json(new { notifications, notificationCount });
                }
            }


            // Action pour marquer une notification comme lue
            [HttpPost]
            public IActionResult MarkNotificationAsRead(int notificationId)
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                try
                {
                    // Log pour déboguer
                    Console.WriteLine($"NotificationId reçu: {notificationId}");

                    // Marquer la notification comme lue
                    _notificationService.MarkNotificationAsRead(notificationId);

                    // Préparer la réponse
                    dynamic response = new { success = true };

                    if (userRole == "user")
                    {
                        response = new { success = true, redirectUrl = Url.Action("Index", "Home") };
                    }
                    else
                    {
                        response = new { success = true, redirectUrl = Url.Action("IndexPage", "Abonnement") };
                    }

                    // Retourner la réponse JSON
                    return Json(response);
                }
                catch (Exception ex)
                {
                    // Log l'erreur
                    Console.WriteLine($"Erreur : {ex.Message}");
                    // Retourner une réponse JSON en cas d'erreur
                    return Json(new { success = false, message = ex.Message });
                }
            }




        }
    }
