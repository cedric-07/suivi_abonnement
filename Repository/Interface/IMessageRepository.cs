using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface IMessageRepository
    {
        List<Message> GetMessagesForConversation(int user1Id, int user2Id);
        int CreateConversation(int user1Id, int user2Id);
        void SendMessage(int senderId, int receiverId, string messageText);
        int GetOrCreateConversation(int senderId, int receiverId);
        User searchUser(string name);
        int CountMessagesisRead(int userId);
        void MarkMessagesAsRead(int receiverId, int senderId);

    }
}