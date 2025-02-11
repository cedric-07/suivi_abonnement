using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using suivi_abonnement.Service.Interface;

namespace suivi_abonnement.Hubs
{
    public class MessageHub : Hub
    {
        private static readonly Dictionary<int, HashSet<string>> ConnectedUsers = new();
        private readonly IMessageService _messageService;

        // 🔥 Correction : Ajout du constructeur pour injecter IMessageService
        public MessageHub(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = httpContext?.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    Console.WriteLine("⚠️ Connexion refusée: Session invalide.");
                    return;
                }

                lock (ConnectedUsers)
                {
                    if (!ConnectedUsers.ContainsKey(userId.Value))
                    {
                        ConnectedUsers[userId.Value] = new HashSet<string>();
                    }
                    ConnectedUsers[userId.Value].Add(Context.ConnectionId);
                }

                Console.WriteLine($"✅ Utilisateur {userId.Value} connecté.");
                await Clients.Others.SendAsync("UserConnected", userId.Value);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans OnConnectedAsync : {ex.Message}");
            }
        }

        // 🔥 Assure-toi que la méthode existe et est publique
        public async Task SendMessageToReceiver(int receiverId, string message)
        {
            try
            {
                var senderId = Context.GetHttpContext()?.Session.GetInt32("UserId");

                if (!senderId.HasValue || string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"❌ Erreur SignalR: senderId ou message invalide. senderId: {senderId}, message: '{message}'");
                    return;
                }

                Console.WriteLine($"📩 Message reçu de {senderId.Value} à {receiverId}: {message}");

                if (_messageService == null)
                {
                    Console.WriteLine("⚠️ Erreur: _messageService est NULL. Vérifiez l'injection de dépendance.");
                    return;
                }

                _messageService.SendMessage(senderId.Value, receiverId, message);

                if (ConnectedUsers.ContainsKey(receiverId))
                {
                    foreach (var connectionId in ConnectedUsers[receiverId])
                    {
                        Console.WriteLine($"🔗 Envoi du message à {connectionId} via SignalR");
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId.Value, message);
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Utilisateur {receiverId} n'est pas en ligne.");
                }

                await Clients.Others.SendAsync("NotifyNewMessage", receiverId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans SendMessage (SignalR) : {ex.Message}");
            }
        }

    }
}
