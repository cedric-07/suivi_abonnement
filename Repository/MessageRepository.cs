using MySql.Data.MySqlClient;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Models;

namespace suivi_abonnement.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository()
        {
            _connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
        }

        public List<Message> GetMessagesForConversation(int user1Id, int user2Id)
        {
            List<Message> messages = new List<Message>();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT * 
                        FROM messages 
                        WHERE idconversation IN (
                            SELECT conversation_id 
                            FROM conversations 
                            WHERE (user1_id = @user1Id AND user2_id = @user2Id) 
                               OR (user1_id = @user2Id AND user2_id = @user1Id)
                        ) 
                        ORDER BY sentat ASC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user1Id", user1Id);
                        command.Parameters.AddWithValue("@user2Id", user2Id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                messages.Add(new Message
                                {
                                    Id = reader.GetInt32("message_id"),
                                    MessageText = reader.GetString("messagetext"),
                                    DateEnvoi = reader.GetDateTime("sentat"),
                                    IsRead = reader.GetBoolean("isread"),
                                    SenderId = reader.GetInt32("senderid"),
                                    ReceiverId = reader.GetInt32("receiverid"),
                                    ConversationId = reader.GetInt32("idconversation")
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de la récupération des messages : {ex.Message}");
            }

            return messages;
        }

        public int CreateConversation(int user1Id, int user2Id)
        {
            int conversationId = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO conversations (user1_id, user2_id, LastMessageat) VALUES (@user1Id, @user2Id, NOW()); SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user1Id", user1Id);
                        command.Parameters.AddWithValue("@user2Id", user2Id);
                        conversationId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de la création de la conversation : {ex.Message}");
            }
            return conversationId;
        }

        public void SendMessage(int senderId, int receiverId, string messageText)
        {
            if (senderId == 0 || receiverId == 0)
            {
                throw new ArgumentException("SenderId ou ReceiverId non valide.");
            }

            int conversationId = GetOrCreateConversation(senderId, receiverId);

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            // Insertion du message
                            string query = @"
                                INSERT INTO messages (senderid, receiverid, messagetext, sentat, isread, idconversation) 
                                VALUES (@senderId, @receiverId, @messageText, NOW(), false, @conversationId)";

                            using (var command = new MySqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@senderId", senderId);
                                command.Parameters.AddWithValue("@receiverId", receiverId);
                                command.Parameters.AddWithValue("@messageText", messageText);
                                command.Parameters.AddWithValue("@conversationId", conversationId);
                                command.ExecuteNonQuery();
                            }


                            // Mise à jour de l'heure du dernier message dans la conversation
                            string updatequery = "UPDATE conversations SET LastMessageat = NOW() WHERE conversation_id = @conversationId";
                            using (var updatecommand = new MySqlCommand(updatequery, connection, transaction))
                            {
                                updatecommand.Parameters.AddWithValue("@conversationId", conversationId);
                                updatecommand.ExecuteNonQuery();
                            }

                            // Validation de la transaction
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // En cas d'erreur, on annule la transaction
                            transaction.Rollback();
                            throw new System.Exception($"Erreur lors de l'envoi du message : {ex.Message}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de l'envoi du message : {ex.Message}");
            }
        }


        public int GetOrCreateConversation(int senderId, int receiverId)
        {
            int conversationId = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT conversation_id
                        FROM conversations 
                        WHERE (user1_id = @user1Id AND user2_id = @user2Id) 
                           OR (user1_id = @user2Id AND user2_id = @user1Id)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user1Id", senderId);
                        command.Parameters.AddWithValue("@user2Id", receiverId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                conversationId = reader.GetInt32("conversation_id");
                            }
                        }

                        if (conversationId == 0)
                        {
                            // Créez une nouvelle conversation si elle n'existe pas
                            conversationId = CreateConversation(senderId, receiverId);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new System.Exception($"Erreur MySQL lors de la récupération de la conversation : {ex.Message}");
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur générale : {ex.Message}");
            }

            return conversationId;
        }

        public User searchUser(string name)
        {
            User user = new User();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT * 
                        FROM users 
                        WHERE username = @name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.Id = reader.GetInt32("id");
                                user.Username = reader.GetString("username");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de la recherche de l'utilisateur : {ex.Message}");
            }

            return user;
        }

        public int CountMessagesisRead(int userId)
        {
            int count = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT COUNT(*) 
                        FROM messages 
                        WHERE receiverid = @userId
                          AND isread = 0";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors du comptage des messages non lus : {ex.Message}");
            }

            return count;
        }

        public void MarkMessagesAsRead(int userId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE messages 
                        SET isread = true 
                        WHERE receiverid = @userId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Erreur lors de la mise à jour des messages en tant que lus : {ex.Message}");
            }
        }
        
    }
}
