using suivi_abonnement.Models;
namespace suivi_abonnement.Service.Interface
{
    public interface IUserService
    {
        User Login(string email, string password);
        string Register(User user, int idDepartement);
        string GeneratePasswordResetToken(string email);
        bool ResetPassword(string token , string newPassword , string email);
        User GetUserByEmail(string email);
        User GetRoleByUser(string role);

    }
}