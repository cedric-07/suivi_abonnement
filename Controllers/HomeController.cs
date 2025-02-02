using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Service;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Controllers
{
    public class HomeController : Controller
    {
         private readonly IAbonnementService _abonnementService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ICategorieService _categorieService;
        private readonly HttpContextAccessor _httpContextAccessor;
        private readonly IMessageService _messageService;
        private readonly UserService _userService;
        private readonly AbonnementViewModel abonnementViewModel = new AbonnementViewModel();
        private readonly AbonnementStatViewModel abonnementStatViewModel = new AbonnementStatViewModel();
        private readonly INotificationService _notificationService;

        private readonly IDepartementService _departementService;
        private readonly User user = new User();
        private readonly ILogger<HomeController> _logger;

        public HomeController(IAbonnementService abonnementService, IFournisseurService fournisseurService, ICategorieService categorieService, INotificationService notificationService, IDepartementService departementService, ILogger<HomeController> logger)
        {
            _abonnementService = abonnementService;
            _fournisseurService = fournisseurService;
            _categorieService = categorieService;
            _notificationService = notificationService;
            _departementService = departementService;
            _logger = logger;
        }


        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            Console.WriteLine(userId);
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userId == 0)
            {
                Console.WriteLine("Utilisateur non connecté");
            }

            _notificationService.SendNotification();
            List<Notification> notifications = new List<Notification>();
            
            if (userRole == "user")
            {
                notifications = _notificationService.GetNotificationsForClient();
            }

            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("Aucune notification trouvée.");
            }
            
            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;
            ViewBag.NotificationCount = notificationCount;
            ViewBag.Notifications = notifications;
            return View();
        }

        public ActionResult IndexPage(string? keyword = null, DateTime? DateDebut = null, DateTime? ExpirationDate = null, string? type = null, int? idcategorie = null, int pageNumber = 1)
        {
            int pageSize = 6; // Nombre d'abonnements par page
            List<Abonnement> abonnements;
            int userId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            Console.WriteLine("User ID : " + userId);
            // Vérification si les dates sont valides
            if (DateDebut.HasValue && ExpirationDate.HasValue)
            {
                if (DateDebut.Value > ExpirationDate.Value)
                {
                    // Afficher un message d'erreur ou rediriger
                    TempData["Error"] = "La date de début ne peut pas être après la date d'expiration.";
                    abonnements = new List<Abonnement>();  // Liste vide en cas d'erreur
                }
                else
                {
                    abonnements = _abonnementService.FiltreDate(DateDebut.Value, ExpirationDate.Value , userId);
                }
            }
            else if (!string.IsNullOrEmpty(type))
            {
                abonnements = _abonnementService.FiltreType(type , userId );
            }
            else if (idcategorie.HasValue)
            {
                abonnements = _abonnementService.FiltreCategorie(idcategorie.Value , userId);
            }
            else
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    abonnements = _abonnementService.getAbonnementByUser(pageNumber, pageSize , userId);
                }
                else
                {
                    abonnements = _abonnementService.searchMultiplyMot(keyword , userId);
                }
            }

            
            List<Fournisseur> fournisseurs = _fournisseurService.GetFournisseurs();
            List<Categorie> categories = _categorieService.GetCategories();
            List<VAbonnementClient> abonnementExpiredOnMonth = _abonnementService.getAbonnementsExpiredOnMonthClient();


            
            int totalAbonnements = _abonnementService.CountTotalAbonnements();
            int totalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);

            var viewModel = new AbonnementViewModel
            {
                Abonnements = abonnements,
                Fournisseurs = fournisseurs,
                Categories = categories,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalAbonnements = totalAbonnements,
                AbonnementClients = abonnementExpiredOnMonth,
                User = user
            };
           

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAbonnements = totalAbonnements;

            return View("~/Views/Home/IndexPage.cshtml", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ContactPage()
        {
            return View();
        }


        

    }
}
