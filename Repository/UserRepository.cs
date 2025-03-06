using System;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
namespace suivi_abonnement.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserRepository(IConfiguration configuration , IHttpContextAccessor httpContextAccessor)
        {
            connectionString = "server=localhost;port=3306;database=suivi_abonnement_omnis_db;user=root;password=;SslMode=None";
            _httpContextAccessor = httpContextAccessor;
        }

        public User Login(string email, string password)
        {
            User user = new User();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM users WHERE email = @Email";
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["password"]?.ToString() ?? string.Empty;
                                if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                                {
                                    user = new User
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Username = reader["username"].ToString(),
                                        Email = reader["email"].ToString(),
                                        Role = reader["role"]?.ToString() ?? string.Empty,
                                        IsConnected = reader.GetBoolean("isconnected")
                                    };

                                    UpdateUserConnectionStatus(user.Id, true);

                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return user;
        }

        private void UpdateUserConnectionStatus(int userId, bool isConnected)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "UPDATE users SET isconnected = @IsConnected WHERE id = @UserId";
                        command.Parameters.AddWithValue("@IsConnected", isConnected);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public string Register(User user, int idDepartement)
        {
            try
            {
                // Afficher les valeurs des propriétés de l'utilisateur
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Password: {user.Password}");
                Console.WriteLine($"departement: {idDepartement}");

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new MySqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;
                                
                                // Insertion dans la table users
                                command.CommandText = "INSERT INTO users (username, email, password, role ) VALUES (@Username, @Email, @Password, @Role )";
                                command.Parameters.AddWithValue("@Username", user.Username);
                                command.Parameters.AddWithValue("@Password", hashedPassword);
                                command.Parameters.AddWithValue("@Email", user.Email);
                                command.Parameters.AddWithValue("@Role", user.Role);
                                command.ExecuteNonQuery();
                                
                                // Récupérer l'ID de l'utilisateur nouvellement inséré
                                command.CommandText = "SELECT LAST_INSERT_ID()";
                                int userId = Convert.ToInt32(command.ExecuteScalar());

                                // Insertion dans la table departement_user
                                command.CommandText = "INSERT INTO departement_user (user_id, iddepartement) VALUES (@UserId, @IdDepartement)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@UserId", userId);
                                command.Parameters.AddWithValue("@IdDepartement", idDepartement);
                                command.ExecuteNonQuery();
                                
                                transaction.Commit();
                                Console.WriteLine("Registration successful for user: " + user.Username + ".");
                                return "Registration successful.";
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Transaction failed: {ex.Message}");
                            return "Registration failed: " + ex.Message;
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
                return sqlEx.Message;
            }
        }

        public string GeneratePasswordResetToken(string email)
        {
            string token = Guid.NewGuid().ToString();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "UPDATE users SET password_reset_token = @token WHERE email = @email";
                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@email", email);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                       Console.WriteLine("Password reset token generated successfully.");
                       return token;
                    }
                    else
                    {
                        Console.WriteLine("Password reset token generation failed.");
                        return string.Empty;
                    }
                }
            }
        }

        public bool ResetPassword(string token , string newPassword , string email)
        {
            using(var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "UPDATE users SET password = @newPassword WHERE  email = @email";
                    command.Parameters.AddWithValue("@newPassword", BCrypt.Net.BCrypt.HashPassword(newPassword));
                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@email", email);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Password reset successful.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Password reset failed.");
                        return false;
                    }
                }
            }
        }

        public void Logout(int userId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = "UPDATE Users SET IsConnected = @IsConnected WHERE Id = @UserId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IsConnected", false);
                    command.Parameters.AddWithValue("@UserId", userId);

                    command.ExecuteNonQuery();
                }
            }
        }


        //Get User by email
        public User GetUserByEmail(string email)
        {
            User user = new User();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM users WHERE email = @Email";
                        command.Parameters.AddWithValue("@Email", email);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            return user;
        }

        public User GetRoleByUser(string role)
        {
            User user = new User();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT * FROM users WHERE role = @Role", connection))
                    {
                        command.Parameters.Add("@Role", MySqlDbType.VarChar).Value = role;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                // Remplacer Console.WriteLine par un log approprié si nécessaire
                Console.WriteLine($"Database error: {sqlEx.Message}");
                // Loggez l'erreur dans un fichier ou un service de log en production
            }
            catch (Exception ex)
            {
                // Catch des exceptions générales pour éviter que l'application plante
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            // Si aucun utilisateur n'est trouvé, la valeur de user restera null
            return user;
        }


        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT * FROM users WHERE role != 'admin'", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"]?.ToString() ?? string.Empty,
                                    IsConnected = reader.GetBoolean("isconnected")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return users;
        }

        public User GetUserById(int id)
        {
            User user = new User();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT * FROM users WHERE id = @Id AND role != 'admin'", connection))
                    {
                        command.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return user;
        }
        public List<User> GetAdmin()
        {
            List<User> adminUser = new List<User>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM users WHERE role = 'admin' LIMIT 1";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                adminUser.Add(new User
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Role = reader["role"]?.ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                
                throw new System.Exception("Admin introuvable");
            }
            return adminUser;
        }

       public string GetUserEmail(int userId)
        {
            string email = string.Empty;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("SELECT email FROM users WHERE id = @Id", connection))
                    {
                        command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                email = reader["email"].ToString();
                            }
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return email;
        }

       public List<Dictionary<string, object>> GetLastinsertedUser()
        {
            List<Dictionary<string, object>> lastInsertedUser = new List<Dictionary<string, object>>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM users ORDER BY id DESC LIMIT 1";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> user = new Dictionary<string, object>();
                                user.Add("id", reader["id"]);
                                user.Add("username", reader["username"]);
                                user.Add("email", reader["email"]);
                                user.Add("role", reader["role"]);
                                lastInsertedUser.Add(user);
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {

                throw new System.Exception("Utilisateur introuvable");
            }
            return lastInsertedUser;
        }

        public User GetUserByEmailOrUsername(string email, string username)
        {
            User user = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM users WHERE email = @Email OR username = @Username LIMIT 1";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                Password = reader["password"].ToString()
                            };
                        }
                    }
                }
            }
            return user;
        }

    }

}