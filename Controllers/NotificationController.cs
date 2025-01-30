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

                    if (userRole == "Admin")
                    {
                        response = new { success = true, redirectUrl = Url.Action("IndexPage", "Abonnement") };
                    }
                    else
                    {
                        response = new { success = true, redirectUrl = Url.Action("Index", "Home") };
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
