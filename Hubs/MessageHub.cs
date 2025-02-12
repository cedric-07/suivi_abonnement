﻿using System;
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

        

        public async Task SendMessageToReceiver(int receiverId, string message)
        {
            try
            {
                var senderId = Context.GetHttpContext()?.Session.GetInt32("UserId");

                if (!senderId.HasValue || string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"❌ [SendMessageToReceiver] Erreur SignalR: senderId ou message invalide. senderId: {senderId}, message: '{message}'");
                    return;
                }

                Console.WriteLine($"📩 [SendMessageToReceiver] Message reçu de {senderId.Value} à {receiverId}: {message}");

                _messageService.SendMessage(senderId.Value, receiverId, message);

                if (ConnectedUsers.ContainsKey(receiverId))
                {
                    foreach (var connectionId in ConnectedUsers[receiverId])
                    {
                        Console.WriteLine($"🔗 [SendMessageToReceiver] Envoi du message à {connectionId} via SignalR");
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId.Value, message);
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ [SendMessageToReceiver] Utilisateur {receiverId} n'est pas en ligne.");
                }

                // 🔥 Toujours envoyer `NotifyNewMessage` si la conversation n'est pas ouverte
                await Clients.User(receiverId.ToString()).SendAsync("NotifyNewMessage", senderId.Value);
                Console.WriteLine($"📬 [SendMessageToReceiver] SignalR - NotifyNewMessage envoyé à l'utilisateur {receiverId} (de {senderId.Value})");

                // 🔥 Envoyer le nombre de messages non lus
                await SendUnreadMessagesCount(receiverId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SendMessageToReceiver] Erreur dans SendMessage (SignalR) : {ex.Message}");
            }
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
