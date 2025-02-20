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
                                string? filePath = reader.IsDBNull(reader.GetOrdinal("file_path")) ? null : reader.GetString("file_path");
                                Console.WriteLine($"DEBUG: filePath = {filePath}"); // 🔍 Vérifie la valeur

                                messages.Add(new Message
                                {
                                    Id = reader.GetInt32("message_id"),
                                    MessageText = reader.GetString("messagetext"),
                                    DateEnvoi = reader.GetDateTime("sentat"),
                                    IsRead = reader.GetBoolean("isread"),
                                    SenderId = reader.GetInt32("senderid"),
                                    ReceiverId = reader.GetInt32("receiverid"),
                                    ConversationId = reader.GetInt32("idconversation"),
                                    filePath = filePath
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

        private static bool isImage(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            return imageExtensions.Contains(Path.GetExtension(filePath));
        }

        private static bool isFile(string filePath)
        {
            string[] fileExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            return fileExtensions.Contains(Path.GetExtension(filePath));
        }

        public int DetectedFile(string filePath)
        {
            switch (filePath)
            {
                case string a when isImage(a):
                    return 1;
                case string b when isFile(b):
                    return 2;
                default:
                    return 0;
            }
        }

        public string UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Fichier non valide.");
            }

            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Extension de fichier non autorisée.");
            }

            // Limiter la taille du fichier (ex: 5 Mo)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                throw new ArgumentException("Fichier trop volumineux.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                Console.WriteLine($"✅ Fichier uploadé : {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur d'upload : {ex.Message}");
                return string.Empty;
            }

            return $"/uploads/{uniqueFileName}"; // Retourne l'URL relative pour la BDD
        }


        public void SendMessage(int senderId, int receiverId, string messageText, string filePath)
        {
            if (senderId == 0 || receiverId == 0)
            {
                throw new ArgumentException("SenderId ou ReceiverId non valide.");
            }

            if (string.IsNullOrWhiteSpace(messageText) && string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Impossible d'envoyer un message vide sans fichier.");
            }

            int conversationId = GetOrCreateConversation(senderId, receiverId);
            int messageType = 0;

            // 📂 Si un fichier est présent, on détecte son type ici !
            if (!string.IsNullOrEmpty(filePath))
            {
                messageType = DetectedFile(filePath);
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = @"
        INSERT INTO messages (senderid, receiverid, messagetext, sentat, isread, idconversation, file_path, message_type)
        VALUES (@senderId, @receiverId, @messageText, NOW(), false, @conversationId, @filePath, @messageType)";

                            using (var command = new MySqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@senderId", senderId);
                                command.Parameters.AddWithValue("@receiverId", receiverId);
                                command.Parameters.AddWithValue("@messageText", (object)messageText ?? DBNull.Value);
                                command.Parameters.AddWithValue("@conversationId", conversationId);
                                command.Parameters.AddWithValue("@filePath", (object)filePath ?? DBNull.Value);
                                command.Parameters.AddWithValue("@messageType", messageType);
                                command.ExecuteNonQuery();
                            }

                            string updateQuery = "UPDATE conversations SET LastMessageat = NOW() WHERE conversation_id = @conversationId";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@conversationId", conversationId);
                                updateCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                                if (File.Exists(fullPath))
                                {
                                    File.Delete(fullPath);
                                }
                            }
                            throw new Exception($"Erreur lors de l'envoi du message : {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'envoi du message : {ex.Message}");
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

        public void MarkMessagesAsRead(int receiverId, int senderId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE messages 
                        SET isread = true 
                        WHERE receiverid = @receiverId AND senderid = @senderId AND isread = false";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@receiverId", receiverId);
                        command.Parameters.AddWithValue("@senderId", senderId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Erreur lors de la mise à jour des messages en tant que lus : {ex.Message}");
            }
        }

        
    }
}
