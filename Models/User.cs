using MySql.Data.MySqlClient;
using System;
using System.Net.Mail;
using System.Net;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace suivi_abonnement.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }   
        public string Role { get; set; } = "user";
        public bool IsConnected { get; set; } = false;
    }
}
