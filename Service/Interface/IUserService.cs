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
        List<User> GetAllUsers();
        User GetUserById(int id);
        List<User> GetAdmin();
        void Logout(int userId);   
        string GetUserEmail(int userId);
        List<Dictionary<string, object>> GetLastinsertedUser();
        User GetUserByEmailOrUsername(string email, string username);



    }
}