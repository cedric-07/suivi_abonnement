using System;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
namespace suivi_abonnement.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly HttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public NotificationController(INotificationRepository notificationRepository , IUserRepository userRepository , HttpContextAccessor httpContextAccessor)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public IActionResult IndexNotification()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var notificationforAdmin = _notificationRepository.GetNotificationsForAdmin();
            var notificationforClient = _notificationRepository.GetNotificationsForClient(userId);
            Console.WriteLine("User Role : " + userRole);
            Console.WriteLine("User Id : " + userId);
            Console.WriteLine("Notification for Admin : " + notificationforAdmin.Count);
            Console.WriteLine("Notification for Client : " + notificationforClient.Count);
            if (userRole != null)
            {
                if(userRole == "admin")
                {
                    ViewBag.NotificationCount = notificationforAdmin.Count;
                    return View("~/Views/Shared/Index.cshtml" , notificationforAdmin);
                }
                else 
                {
                    ViewBag.NotificationCount = notificationforClient.Count;
                    return View("~/Views/Shared/LayoutClient/_LayoutClient.cshtml" , notificationforClient);
                }
            }
            return View("~/Views/Shared/Index.cshtml");
        }
    }
}