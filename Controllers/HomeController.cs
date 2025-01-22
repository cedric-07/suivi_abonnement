using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Controllers
{
    public class HomeController : Controller
    {
         private readonly IAbonnementService _abonnementService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ICategorieService _categorieService;
        private readonly AbonnementViewModel abonnementViewModel = new AbonnementViewModel();
        private readonly AbonnementStatViewModel abonnementStatViewModel = new AbonnementStatViewModel();

        private readonly IDepartementService _departementService;
        private readonly User user = new User();
        private readonly ILogger<HomeController> _logger;

        public HomeController(IAbonnementService abonnementService, IFournisseurService fournisseurService, ICategorieService categorieService, ILogger<HomeController> logger, IDepartementService departementService)
        {
            _abonnementService = abonnementService;
            _fournisseurService = fournisseurService;
            _categorieService = categorieService;
            _logger = logger;
            _departementService = departementService;
        }


        public IActionResult Index()
        {
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

            
            int totalAbonnements = _abonnementService.CountTotalAbonnements();
            int totalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);

            var viewModel = new AbonnementViewModel
            {
                Abonnements = abonnements,
                Fournisseurs = fournisseurs,
                Categories = categories,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalAbonnements = totalAbonnements
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
