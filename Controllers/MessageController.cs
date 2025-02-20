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

       
        [HttpPost("Message/UploadFile")] // 🔥 Route explicite
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier sélectionné.");
            }

            try
            {
                string filePath = await Task.Run(() => _messageService.UploadFile(file)); // 🔥 Stocke et retourne le chemin

                if (string.IsNullOrEmpty(filePath))
                {
                    return StatusCode(500, "Le fichier n'a pas pu être enregistré.");
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'upload : {ex.Message}");
            }
        }

        [HttpGet("uploads/{filename}")]
        public IActionResult GetUploadFile(string filename)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Le fichier n'existe pas.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filePath);
            return File(fileBytes, contentType, filename);
        }
        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".txt", "text/plain" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        [HttpGet]
        public IActionResult Index(int? receiverId)
        {
            try
            {
                int userId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId") ?? 0;
                var userRole = _httpContextAccessor.HttpContext?.Session?.GetString("UserRole");

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
                Console.WriteLine($"❌ Erreur dans Index: {ex.Message}");
                TempData["Error"] = "Une erreur s'est produite lors du chargement des messages.";
                return RedirectToAction("Index");
            }
        }

       

        [HttpPost("Message/SendMessage")]
        public async Task<IActionResult> SendMessageToReceiver(int receiverId, string messageText , IFormFile file)
        {
            try
            {
                int senderId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId") ?? 0;

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
                string? filePath = null;
                if (file != null && file.Length > 0)
                {
                    filePath = _messageService.UploadFile(file);
                }
                _messageService.SendMessage(senderId, receiverId, messageText, filePath?? string.Empty);

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
            var userRole = _httpContextAccessor.HttpContext?.Session?.GetString("UserRole");
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
                int userId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId") ?? 0;
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
            int userId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId") ?? 0;
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
