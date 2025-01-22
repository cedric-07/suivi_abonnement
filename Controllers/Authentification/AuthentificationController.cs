using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace suivi_abonnement_omnis.Controllers.Authentification
{
    public class AuthentificationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartementService _departementService;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        
        public AuthentificationController(IConfiguration configuration , IHttpContextAccessor httpContextAccessor , IUserService userService , IDepartementService departementService)
        {
            _userService = userService;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            _departementService = departementService;
        }

        // GET: AuthController
        [HttpGet]
        public IActionResult Login()
        {
            return View(new User());
        }

        // POST: AuthController/Login
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
                        case "admin":
                            // Redirection vers l'action AbonnementPage
                            return RedirectToAction("Index", "Abonnements");
                        default:
                            TempData["Error"] = "Rôle utilisateur inconnu.";
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




        // GET: AuthController/Register
        [HttpGet]
        public IActionResult Register()
        {
            List<Departement> departements = _departementService.getDepartements();
            ViewBag.Departements = departements;
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, int idDepartement)
        {
            if (ModelState.IsValid)
            {
                var userByEmail = _userService.GetUserByEmail(user.Email);
                if (userByEmail != null)
                {
                    TempData["Error"] = "Un utilisateur avec cet email existe déjà.";
                    return RedirectToAction("Register", "Authentification");
                }
                // Appeler la méthode Register du modèle User avec idDepartement
                var result = _userService.Register(user, idDepartement);
                if (result == "Registration successful.")
                {
                    return RedirectToAction("Login", "Authentification");
                }
                else
                {
                    TempData["Error"] = result;
                    return View(user);
                }
            }
            return View(user);
        }


        // GET: AuthController/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: AuthController/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {  
            var userByEmail = _userService.GetUserByEmail(email);
            if (userByEmail == null)
            {
                TempData["Message"] = "Email introuvable. Veuillez vérifier votre email.";
                TempData["ShowModal"] = true;
                return View("login");
            }
            // Génération du token de réinitialisation si l'email existe
            var token = _userService.GeneratePasswordResetToken(email);
            if (token == null)
            {
                TempData["Message"] = "Une erreur est survenue lors de la création du lien de réinitialisation du mot de passe.";
                return View();
            }

            // Si le token est généré, on envoie l'email
            var resetUrl = Url.Action("ResetPassword", "Authentification", new { token = token, email = email }, Request.Scheme);
            var smtpServer = configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
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
            return RedirectToAction("Login", "Authentification");
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
            //Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.
            string password = model.NewPassword;
            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"))
            {
                TempData["Error"] = "Le mot de passe doit contenir au moins 8 caractères, une lettre majuscule, une lettre minuscule, un chiffre et un caractère spécial.";
                return View(model);
            }
            // Vérifier si les mots de passe correspondent
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Les mots de passe ne correspondent pas.";
                return View(model);
            }

            bool result = _userService.ResetPassword(model.Token, model.NewPassword, model.Email);

            if (!result)
            {
                TempData["Error"] = "Réinitialisation de mot de passe échouée.";
                return View(model);
            }

            TempData["Message"] = "Réinitialisation de mot de passe réussie.";
            return RedirectToAction("Login", "Authentification");
        }

        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Authentification");
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
