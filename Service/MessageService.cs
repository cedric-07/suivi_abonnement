using System.Collections.Generic;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Service
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        public List<Message> GetMessagesForConversation(int senderId, int receiverId)
        {
            return _messageRepository.GetMessagesForConversation(senderId, receiverId);
        }
        public int CreateConversation(int senderId, int receiverId)
        {
            return _messageRepository.CreateConversation(senderId, receiverId);
        }
        public void SendMessage(int senderId, int receiverId, string messageText , string fileUrl = null)
        {
            _messageRepository.SendMessage(senderId, receiverId, messageText, fileUrl);
        }
        public int GetOrCreateConversation(int senderId, int receiverId)
        {
            return _messageRepository.GetOrCreateConversation(senderId, receiverId);
        }
        public User searchUser(string name)
        {
            return _messageRepository.searchUser(name);
        }
        public int CountMessagesisRead(int userId)
        {
            return _messageRepository.CountMessagesisRead(userId);
        }
        public void MarkMessagesAsRead(int receiverId, int senderId)
        {
            _messageRepository.MarkMessagesAsRead(receiverId, senderId);
        }
       
    }
}