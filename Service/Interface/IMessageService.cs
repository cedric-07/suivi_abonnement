using suivi_abonnement.Models;
namespace suivi_abonnement.Service.Interface
{
    public interface IMessageService
    {
        List<Message> GetMessagesForConversation(int senderId, int receiverId);
        int CreateConversation(int senderId, int receiverId);
        void SendMessage(int senderId, int receiverId, string messageText);
        int GetOrCreateConversation(int senderId, int receiverId);
        User searchUser(string name);   
        int CountMessagesisRead(int userId);
        void MarkMessagesAsRead(int userId);

    }
}