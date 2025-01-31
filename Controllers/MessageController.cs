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

                var model = new MessageViewModel
                {
                    Users = users,
                    Messages = messages,
                    ReceiverId = receiverId
                };

                return View("~/Views/AdminPage/MessagePage.cshtml", model);
            }
            catch (Exception ex)
            {
                // Log the exception (ajoutez votre logique de journalisation ici)
                TempData["Error"] = "Une erreur s'est produite lors du chargement des messages. Veuillez réessayer plus tard.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SendMessage(int receiverId, string messageText)
        {
            try
            {
                int senderId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;

                // Vérifiez si l'utilisateur est connecté
                if (senderId == 0)
                {
                    return RedirectToAction("Login", "Account"); // Remplacez "Account" et "Login" par vos valeurs réelles
                }

                // Validation du message
                if (string.IsNullOrEmpty(messageText))
                {
                    TempData["Error"] = "Le message ne peut pas être vide.";
                    return RedirectToAction("Index", new { receiverId });
                }

                _messageService.SendMessage(senderId, receiverId, messageText);
                return RedirectToAction("Index", new { receiverId });
            }
            catch (Exception ex)
            {
                // Log the exception (ajoutez votre logique de journalisation ici)
                Console.WriteLine("Une erreur s'est produite lors de l'envoi du message : " + ex.Message); 
                return RedirectToAction("Index", new { receiverId });
            }
        }
    }
}
