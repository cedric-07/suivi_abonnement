using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using suivi_abonnement.Models;
using suivi_abonnement.Service;
namespace suivi_abonnement.Controllers
{
    public class NotificationFilter : IActionFilter
    {
        private readonly NotificationService _notificationService;

        public NotificationFilter(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = (Controller)context.Controller;

            int userId = controller.HttpContext.Session.GetInt32("UserId") ?? 0;
            var userRole = controller.HttpContext.Session.GetString("UserRole");

            List<Notification> notifications = userRole == "admin"
                ? _notificationService.GetNotificationsForAdmin()
                : new List<Notification>();

            controller.ViewBag.Notifications = notifications;
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

}