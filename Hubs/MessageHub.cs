using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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






        //public async Task SendMessageToReceiver(int receiverId, string message)
        //{
        //    try
        //    {
        //        var senderId = Context.GetHttpContext()?.Session.GetInt32("UserId");

        //        if (!senderId.HasValue || string.IsNullOrEmpty(message))
        //        {
        //            Console.WriteLine($"❌ [SendMessageToReceiver] Erreur SignalR: senderId ou message invalide. senderId: {senderId}, message: '{message}'");
        //            return;
        //        }

        //        // 🔥 Convertir les liens AVANT d'envoyer le message
        //        string formattedMessage = ConvertLinksToHtmlLinks(message);

        //        Console.WriteLine($"📩 [SendMessageToReceiver] Message formaté de {senderId.Value} à {receiverId}: {formattedMessage}");

        //        // 🔥 Enregistrer en base avec le message formaté (si nécessaire)
        //        _messageService.SendMessage(senderId.Value, receiverId, formattedMessage);

        //        // 🔥 Envoyer le message converti au destinataire
        //        if (ConnectedUsers.ContainsKey(receiverId))
        //        {
        //            foreach (var connectionId in ConnectedUsers[receiverId])
        //            {
        //                Console.WriteLine($"🔗 [SendMessageToReceiver] Envoi du message formaté à {connectionId} via SignalR");
        //                await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId.Value, formattedMessage);
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine($"⚠️ [SendMessageToReceiver] Utilisateur {receiverId} n'est pas en ligne.");
        //        }

        //        // 🔥 Toujours envoyer la notification de nouveau message
        //        await Clients.User(receiverId.ToString()).SendAsync("NotifyNewMessage", senderId.Value);
        //        Console.WriteLine($"📬 [SendMessageToReceiver] SignalR - NotifyNewMessage envoyé à l'utilisateur {receiverId} (de {senderId.Value})");

        //        // 🔥 Envoyer le nombre de messages non lus
        //        await SendUnreadMessagesCount(receiverId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ [SendMessageToReceiver] Erreur dans SendMessage (SignalR) : {ex.Message}");
        //    }
        //}


        public async Task SendMessageToReceiver(int receiverId, string message, string? filePath = null)
        {
            try
            {
                var senderId = Context.GetHttpContext()?.Session.GetInt32("UserId");

                if (!senderId.HasValue || (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(filePath)))
                {
                    Console.WriteLine($"❌ [SendMessageToReceiver] Erreur : senderId invalide ou aucun fichier/message.");
                    return;
                }

                // 🔥 Enregistrement du message dans la base de données
                _messageService.SendMessage(senderId.Value, receiverId, message, filePath ?? string.Empty);

                var messageType = !string.IsNullOrEmpty(filePath) ? _messageService.DetectedFile(filePath) : 0;

                var messageData = new
                {
                    senderId = senderId.Value,
                    message = message,
                    filePath = filePath,
                    messageType = messageType
                };

                // 🔥 Envoi en temps réel via SignalR
                if (ConnectedUsers.TryGetValue(receiverId, out var connections))
                {
                    await Clients.Clients(connections).SendAsync("ReceiveMessage", messageData);
                }

                // 🔔 Notification de nouveau message
                await Clients.User(receiverId.ToString()).SendAsync("NotifyNewMessage", senderId.Value);
                await SendUnreadMessagesCount(receiverId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SendMessageToReceiver] Erreur : {ex.Message}");
            }
        }







        //    private string ConvertLinksToHtmlLinks(string messageText)
        //     {
        //         if (string.IsNullOrWhiteSpace(messageText))
        //             return messageText;

        //         // 🔹 Regex amélioré pour capturer aussi les liens sans http/https
        //         string pattern = @"((http|https):\/\/[^\s]+)|(\bwww\.[^\s]+)";

        //         return Regex.Replace(messageText, pattern, match =>
        //         {
        //             string url = match.Value;

        //             // Ajoute "http://" si ce n'est pas un lien absolu
        //             if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        //             {
        //                 url = "http://" + url;
        //             }

        //             return $"<a href=\"{url}\" target=\"_blank\">{match.Value}</a>";
        //         });
        //     }


        private string ConvertLinksToHtmlLinks(string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return messageText;

        string pattern = @"((http|https):\/\/[^\s]+)|(\bwww\.[^\s]+)";

        return Regex.Replace(messageText, pattern, match =>
        {
            string url = match.Value;

            // Ajoute "http://" si ce n'est pas un lien absolu
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            return $"<a href=\"{url}\" target=\"_blank\">{match.Value}</a>";
        });
    }



        public async Task MarkMessagesAsRead(int senderId, int receiverId)
        {
            try
            {
                Console.WriteLine($"✅ [MarkMessagesAsRead] Marquer les messages de {senderId} à {receiverId} comme lus.");

                // 🔥 Mettre à jour les messages en base de données
                _messageService.MarkMessagesAsRead(senderId, receiverId);

                // 🔥 Informer l'expéditeur que ses messages ont été lus
                if (ConnectedUsers.ContainsKey(senderId))
                {
                    foreach (var connectionId in ConnectedUsers[senderId])
                    {
                        await Clients.Client(connectionId).SendAsync("MessagesMarkedAsRead", receiverId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [MarkMessagesAsRead] Erreur : {ex.Message}");
            }
        }


        public async Task SendUnreadMessagesCount(int userId)
        {
            try
            {
                if (userId == 0)
                    return;

                // 🔥 Appel du service pour obtenir le nombre de messages non lus
                int unreadCount = _messageService.CountMessagesisRead(userId);

                Console.WriteLine($"📬 Notification : {unreadCount} messages non lus pour l'utilisateur {userId}");

                // Vérifier si l'utilisateur est connecté
                if (ConnectedUsers.ContainsKey(userId))
                {
                    foreach (var connectionId in ConnectedUsers[userId])
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveUnreadMessagesCount", unreadCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur dans SendUnreadMessagesCount : {ex.Message}");
            }
        }


    }
}
