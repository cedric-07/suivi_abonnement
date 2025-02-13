using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Models;
using suivi_abonnement.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace suivi_abonnement.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessageController(
            IMessageService messageService, 
            IUserService userService, 
            IHttpContextAccessor httpContextAccessor, 
            INotificationService notificationService,
            IHubContext<MessageHub> hubContext)
        {
            _messageService = messageService;
            _userService = userService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
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

            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("Aucune notification trouvée.");
            }

            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;

            ViewBag.Notifications = notifications;
            ViewBag.NbrNotifications = notificationCount;

            base.OnActionExecuting(context);
        }

        [HttpGet]
        public IActionResult Index(int? receiverId)
        {
            try
            {
                int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
                var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var users = _userService.GetAllUsers();
                var adminUser = _userService.GetAdmin();

                var messages = receiverId.HasValue 
                    ? _messageService.GetMessagesForConversation(userId, receiverId.Value) 
                    : new List<Message>();

                // if (receiverId.HasValue)
                // {
                //     _messageService.MarkMessagesAsRead(userId);
                // }

                var model = new MessageViewModel
                {
                    Users = users,
                    Messages = messages,
                    ReceiverId = receiverId,
                    CurrentUserId = userId
                };

                var viewmodel = new AbonnementViewModel
                {
                    MessageViewModel = new MessageViewModel
                    {
                        Users = adminUser,
                        Messages = messages,
                        ReceiverId = receiverId,
                        CurrentUserId = userId
                    }
                };

                return userRole == "admin" 
                    ? View("~/Views/AdminPage/MessagePage.cshtml", model) 
                    : View("~/Views/Home/InboxPage.cshtml", viewmodel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors du chargement des messages.";
                return RedirectToAction("Index");
            }
        }

       

        [HttpPost("Message/SendMessage")]
        public async Task<IActionResult> SendMessageToReceiver(int receiverId, string messageText)
        {
            try
            {
                int senderId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;

                if (senderId == 0)
                {
                    Console.WriteLine("❌ Utilisateur non authentifié.");
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(messageText))
                {
                    Console.WriteLine("⚠️ Message vide.");
                    return BadRequest("Le message ne peut pas être vide.");
                }

                messageText = ConvertLinksToHtmlLinks(messageText);

                Console.WriteLine($"📩 Message envoyé de {senderId} à {receiverId}: {messageText}");

                _messageService.SendMessage(senderId, receiverId, messageText);

                await _hubContext.Clients.User(receiverId.ToString())
                    .SendAsync("ReceiveMessage", senderId, messageText);

                await _hubContext.Clients.User(receiverId.ToString())
                    .SendAsync("NotifyNewMessage", senderId);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans SendMessage: {ex.Message}");
                return StatusCode(500, "Erreur serveur");
            }
        }



        private string ConvertLinksToHtmlLinks(string messageText)
        {
            string pattern = @"(https?://[^\s]+)";
            string replacement = @"<a href=""$1"" target=""_blank"">$1</a>";
            return Regex.Replace(messageText, pattern, replacement);
        }

        [HttpGet("Message/searchUser")]
        public IActionResult searchUser(string name)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            try
            {
                var user = _messageService.searchUser(name);
                if (user != null && user.Id > 0)
                {
                    var model = new MessageViewModel
                    {
                        ReceiverId = user.Id,
                        Users = new List<User> { user }
                    };

                    return userRole == "admin" 
                        ? View("~/Views/AdminPage/MessagePage.cshtml", model) 
                        : View("~/Views/Home/InboxPage.cshtml", model);
                }
                else
                {
                    TempData["Error"] = "Aucun utilisateur trouvé avec ce nom.";
                    return userRole == "admin" 
                        ? View("~/Views/AdminPage/MessagePage.cshtml") 
                        : View("~/Views/Home/InboxPage.cshtml");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de la recherche de l'utilisateur : " + ex.Message);
                return userRole == "admin" 
                    ? View("~/Views/AdminPage/MessagePage.cshtml") 
                    : View("~/Views/Home/InboxPage.cshtml");
            }
        }

        [HttpGet("Message/GetUnreadMessagesCount")]
        public JsonResult GetUnreadMessagesCount()
        {
            try
            {
                int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId == 0)
                    return Json(new { count = 0 });

                int count = _messageService.CountMessagesisRead(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du comptage des messages non lus : " + ex.Message);
                return Json(new { count = 0, error = ex.Message });
            }
        }

        // [HttpPost("Message/MarkMessagesAsRead")]
        // public JsonResult MarkMessagesAsRead()
        // {
        //     try
        //     {
        //         int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
        //         if (userId == 0)
        //             return Json(new { success = false });

        //         _messageService.MarkMessagesAsRead(userId);
        //         return Json(new { success = true });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("Erreur lors du marquage des messages comme lus : " + ex.Message);
        //         return Json(new { success = false });
        //     }
        // }

        [HttpGet]
        public IActionResult GetMessages(int receiverId)
        {
            int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                var messages = _messageService.GetMessagesForConversation(userId, receiverId);
                return Json(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du chargement des messages : " + ex.Message);
                return Json(new { error = ex.Message });
            }
        }



    }
}
