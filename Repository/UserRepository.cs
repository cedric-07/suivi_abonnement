using System;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using MySql.Data.MySqlClient;
namespace suivi_abonnement.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        private readonly HttpContextAccessor _httpContextAccessor;
        public UserRepository(IConfiguration configuration , IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("MySqlConnection");
            httpContextAccessor = _httpContextAccessor;
        }

        public User Login(string email, string password)
        {
            User user = null;
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
                                string storedPassword = reader["password"].ToString();
                                if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                                {
                                    user = new User
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Username = reader["username"].ToString(),
                                        Email = reader["email"].ToString(),
                                        Role = reader["role"].ToString()
                                    };
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

        public string Register(User user, int idDepartement)
        {
            try
            {
                // Afficher les valeurs des propriétés de l'utilisateur
                Console.WriteLine($"Username: {user.Username}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Password: {user.Password}");

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
                                command.CommandText = "INSERT INTO users (username, email, password, role) VALUES (@Username, @Email, @Password, @Role)";
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
                        return null;
                    }
                }
                connection.Close();
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

        //Get User by email
        public User GetUserByEmail(string email)
        {
            User user = null;
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
                                    Role = reader["role"].ToString()
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
    }
}