using suivi_abonnement.Service.Interface;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
namespace suivi_abonnement.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public User Login(string email, string password)
        {
            return _userRepository.Login(email, password);
        }
        public string Register(User user, int idDepartement)
        {
            return _userRepository.Register(user, idDepartement);
        }
        public string GeneratePasswordResetToken(string email)
        {
            return _userRepository.GeneratePasswordResetToken(email);
        }
        public bool ResetPassword(string token, string newPassword, string email)
        {
            return _userRepository.ResetPassword(token, newPassword, email);
        }
        public User GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
        }
        public User GetRoleByUser(string role)
        {
            return _userRepository.GetRoleByUser(role);
        }
    }
}