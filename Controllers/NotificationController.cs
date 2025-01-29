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

                    // Retourner une réponse JSON pour indiquer que l'opération a réussi
                    return Json(new { success = true });
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
