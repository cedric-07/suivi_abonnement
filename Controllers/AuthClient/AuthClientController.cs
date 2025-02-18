using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace suivi_abonnement_omnis.Controllers.Authentification
{
    public class AuthClientController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartementService _departementService;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        
        public AuthClientController(IConfiguration configuration , IHttpContextAccessor httpContextAccessor , IUserService userService , IDepartementService departementService)
        {
            _userService = userService;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            _departementService = departementService;
        }

        // GET: AuthClientController
        [HttpGet]
        public IActionResult Login()
        {
            return View(new User());
        }

        // POST: AuthClientController/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Message"] = "Veuillez fournir un email et un mot de passe valides.";
                return View(new User());
            }

            try
            {
                var user = _userService.Login(email, password);
                
                if (user != null)
                {
                    // Stockage de l'utilisateur dans la session ou tout autre système de gestion d'état si nécessaire
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.Id);

                    switch (user.Role)
                    {
                        case "user":
                            return RedirectToAction("Index", "Home");
                        default:
                            TempData["Error"] = "Vous n'êtes pas autorisé à accéder à cette page , veuillez contacter l'administrateur.";
                            return View(new User());
                    }
                }
                else
                {
                    TempData["Error"] = "Erreur de connexion, veuillez vérifier vos identifiants.";
                    return View(new User());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur est survenue lors de la tentative de connexion. Veuillez réessayer plus tard.";
                Console.WriteLine($"Erreur: {ex.Message}");
                return View(new User());
            }
        }




        // GET: AuthClientController/Register
        [HttpGet]
        public IActionResult Register()
        {
            var departements = _departementService.getDepartements();
            ViewBag.Departements = departements;
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, int idDepartement)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'email existe déjà
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    TempData["Error"] = "L'email ne peut pas être vide.";
                    return View(user);
                }

                var userByEmail = _userService.GetUserByEmail(user.Email);
                if (userByEmail != null)
                {
                    TempData["Error"] = "Un utilisateur avec cet email existe déjà.";
                    return RedirectToAction("Register", "AuthClient");
                }
                //Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.
                string password = user.Password ?? string.Empty;
                if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
                {
                    TempData["Error"] = "Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.";
                    return View(user);
                }
                // Appeler la méthode Register du modèle User avec idDepartement
                var result = _userService.Register(user, idDepartement);
                if (result == "Registration successful.")
                {
                    return RedirectToAction("Login", "AuthClient");
                }
                else
                {
                    TempData["Error"] = result;
                    return View(user);
                }
            }
            return View(user);
        }


        // GET: AuthClientController/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: AuthClientController/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {  
            var userByEmail = _userService.GetUserByEmail(email);
            if (userByEmail == null)
            {
                TempData["Message"] = "Email introuvable. Veuillez vérifier votre email.";
                TempData["ShowModal"] = true;
                return RedirectToAction("Login", "AuthClient");
            }
            // Génération du token de réinitialisation si l'email existe
            var token = _userService.GeneratePasswordResetToken(email);
            if (token == null)
            {
                TempData["Message"] = "Une erreur est survenue lors de la création du lien de réinitialisation du mot de passe.";
                return RedirectToAction("Login", "AuthClient");
            }

            // Si le token est généré, on envoie l'email
            var resetUrl = Url.Action("ResetPassword", "AuthClient", new { token = token, email = email }, Request.Scheme);
            var smtpServer = configuration["EmailSettings:SmtpServer"];
            var smtpPortString = configuration["EmailSettings:SmtpPort"];
            if (string.IsNullOrEmpty(smtpPortString))
            {
                throw new ArgumentNullException("SmtpPort configuration is missing.");
            }
            var smtpPort = int.Parse(smtpPortString);
            var senderEmail = "OMNIS Madagascar <" + configuration["EmailSettings:SenderEmail"] + ">";

            var message = new MailMessage();
            message.From = new MailAddress(senderEmail);
            message.To.Add(new MailAddress(email));
            message.Subject = "Réinitialisation de mot de passe";
            message.Body = $"Bonjour {email}, <br/> Veuillez cliquer sur ce lien pour réinitialiser votre mot de passe : <br/> <a href='{resetUrl}'>Réinitialiser le mot de passe</a>";
            message.IsBodyHtml = true;

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(configuration["EmailSettings:SenderEmail"], configuration["EmailSettings:SenderPassword"]);
                client.EnableSsl = true;
                client.Send(message);
            }

            TempData["Message"] = "Un email de réinitialisation de mot de passe a été envoyé à votre adresse email";
            return RedirectToAction("Login", "AuthClient");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Veuillez remplir tous les champs, s'il vous plaît.";
                return View(model);
            }
            //Le mot de passe doit etre en 8 caractères minimum avec au moins 1 chiffre et 1 lettre majuscule et 1 lettre minuscule et 1 caractère speciaux
            if (!Regex.IsMatch(model.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
            {
                TempData["Error"] = "Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.";
                return View(model);
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Les mots de passe ne correspondent pas.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.Email))
            {
                TempData["Error"] = "Token ou email invalide.";
                return View(model);
            }
            bool result = _userService.ResetPassword(model.Token, model.NewPassword, model.Email);

            if (!result)
            {
                TempData["Error"] = "Réinitialisation de mot de passe échouée.";
                return View(model);
            }

            TempData["Message"] = "Réinitialisation de mot de passe réussie.";
            return RedirectToAction("Login", "AuthClient");
        }

        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }
        public IActionResult Logout()
        {
            // Récupérer l'ID de l'utilisateur depuis la session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                // Appeler le service pour mettre à jour IsConnected sur false
                _userService.Logout(userId.Value);
            }

            // Effacer la session
            HttpContext.Session.Clear();

            // Rediriger vers la page de connexion
            return RedirectToAction("Login", "AuthClient");
        }


        [HttpPost]
        public JsonResult CheckEmailExistence(string email)
        {
            var user = _userService.GetUserByEmail(email); // Utiliser votre méthode GetUserByEmail pour rechercher l'email
            if (user == null)
            {
                return Json(new { exists = false, message = "Email introuvable. Veuillez vérifier votre email." });
            }
            return Json(new { exists = true });
        }


    }
}
