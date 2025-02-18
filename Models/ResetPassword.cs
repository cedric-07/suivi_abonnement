namespace suivi_abonnement.Models
{
    public class ResetPassword
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
    }
}
