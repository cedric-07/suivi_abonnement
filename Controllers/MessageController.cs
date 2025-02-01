using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Models;
using System;

namespace suivi_abonnement.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageController(IMessageService messageService, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _messageService = messageService;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Index(int? receiverId)
        {
            try
            {
                int userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
                var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

                // Si l'utilisateur n'est pas connecté, redirigez-le vers la page de connexion
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account"); // Remplacez "Account" et "Login" par vos valeurs réelles
                }

                // Get users
                var users = _userService.GetAllUsers();

                // Get messages for the selected receiver (if any)
                var messages = receiverId.HasValue
                    ? _messageService.GetMessagesForConversation(userId, receiverId.Value)
                    : new List<Message>();

                 var viewmodel = new AbonnementViewModel
                 {
                    MessageViewModel = new MessageViewModel
                    {
                        Users = users,
                        Messages = messages,
                        ReceiverId = receiverId,
                        CurrentUserId = userId 
                    }
                 };

                 var model = new MessageViewModel
                {
                    Users = users,
                    Messages = messages,
                    ReceiverId = receiverId,
                    CurrentUserId = userId 
                };

                if(userRole == "admin")
                {
                    return View("~/Views/AdminPage/MessagePage.cshtml", model);
                }
                else
                {
                    return View("~/Views/Home/InboxPage.cshtml" , viewmodel);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ajoutez votre logique de journalisation ici)
                TempData["Error"] = "Une erreur s'est produite lors du chargement des messages. Veuillez réessayer plus tard.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Message/SendMessage")]
  
        public IActionResult SendMessage(int receiverId, string messageText, IFormFile attachment, IFormFile image)
        {
            try
            {
                int senderId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;

                if (senderId == 0)
                    return RedirectToAction("Login", "Account");

                if (string.IsNullOrEmpty(messageText) && attachment == null && image == null)
                {
                    TempData["Error"] = "Veuillez fournir un message ou une pièce jointe.";
                    return RedirectToAction("Index", new { receiverId });
                }

                // Gestion des pièces jointes (sauvegarde des fichiers)
                if (attachment != null)
                {
                    var filePath = Path.Combine("wwwroot/uploads", attachment.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        attachment.CopyTo(stream);
                    }
                    messageText += $" [Fichier joint : {attachment.FileName}]";
                }

                _messageService.SendMessage(senderId, receiverId, messageText);
                return RedirectToAction("Index", new { receiverId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'envoi du message : " + ex.Message);
                return RedirectToAction("Index", new { receiverId });
            }
        }

    }
}
