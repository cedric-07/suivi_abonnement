using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using Rotativa.AspNetCore;

namespace suivi_abonnement.Controllers
{
    public class AbonnementsController : Controller
    {
        private readonly IAbonnementService _abonnementService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ICategorieService _categorieService;
        private readonly INotificationService _notificationService;
        private readonly AbonnementViewModel abonnementViewModel = new AbonnementViewModel();
        private readonly AbonnementStatViewModel abonnementStatViewModel = new AbonnementStatViewModel();
        private readonly IHttpContextAccessor _httpContextAccessor;        
        private readonly IDepartementService _departementService;
        private readonly User user = new User();
        public AbonnementsController(IAbonnementService abonnementService, IFournisseurService fournisseurService, ICategorieService categorieService, IDepartementService departementService , INotificationService notificationService , IHttpContextAccessor httpContextAccessor)
        {
            _abonnementService = abonnementService;
            _fournisseurService = fournisseurService;
            _categorieService = categorieService;
            _departementService = departementService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        //Controller de notifications

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var userRole = HttpContext.Session.GetString("UserRole");

            _notificationService.SendNotification();
            List<Notification> notifications = new List<Notification>();

            if (userRole == "admin")
            {
                notifications = _notificationService.GetNotificationsForAdmin();
            }
            else if (userRole == "client")
            {
                notifications = _notificationService.GetNotificationsForClient();
            }

            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("Aucune notification trouvée.");
            }

            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;

            ViewBag.Notifications = notifications;
            ViewBag.NbrNotifications = notificationCount;

            base.OnActionExecuting(context);
        }

        // GET: AbonnementsController
        
        public IActionResult Index()
        {
            // Récupération des informations de session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var userRole = HttpContext.Session.GetString("UserRole");

            // Vérification si l'utilisateur est connecté
            if (string.IsNullOrEmpty(userRole) || userId == 0)
            {
                Console.WriteLine("Utilisateur non connecté");
            }

            // Envoi de notifications (si nécessaire)
            _notificationService.SendNotification();


            // Initialisation de la liste de notifications
            List<Notification> notifications = new List<Notification>();

            // Récupération des notifications selon le rôle
            if (userRole == "admin")
            {
                notifications = _notificationService.GetNotificationsForAdmin();
            }

            if (notifications == null || !notifications.Any())
            {
                Console.WriteLine("Aucune notification trouvée.");
            }
            

            // Calcul du nombre de notifications non lues
            int notificationCount = notifications?.Count(n => n.Status == "non lu") ?? 0;

            // Passage du nombre de notifications à la vue
            ViewBag.NotificationCount = notificationCount;

            // Appel des méthodes pour obtenir les statistiques d'abonnements
            int abonnementsActifs = _abonnementService.CountTotalAbonnementsActif();
            int abonnementsExpirés = _abonnementService.CountTotalAbonnementsInactif();
            int abonnementsSuspendus = _abonnementService.CountTotalAbonnementsEnAttente();

            // Appel des méthodes pour obtenir les revenus fictifs
            List<Dictionary<string, object>> revenusAnnuels = _abonnementService.RevenusFictifsParAnnee();
            List<Dictionary<string, object>> revenusMensuels = _abonnementService.RevenusFictifsParMois();
            List<Abonnement> abonnementfournisseur = _abonnementService.GetNbrAbonnementPerFournisseur();
            foreach (var item in abonnementfournisseur)
            {
                Console.WriteLine("${item.NomFournisseur} : {item.NbrAbonnement}");
            }
            int nbrlcient = _abonnementService.NbrClientAbonne();
            
            
            //ViewBag pour le notification
            ViewBag.Notifications = notifications;
            ViewBag.NbrNotifications = notificationCount;
            //ViewBag pour les statistiques
            // Création du modèle à passer à la vue
            var model = new GlobalViewModel
            {
                AbonnementStatViewModel = new AbonnementStatViewModel
                {
                    Actifs = abonnementsActifs,
                    Expirés = abonnementsExpirés,
                    Suspendus = abonnementsSuspendus,
                    RevenusAnnuels = revenusAnnuels,
                    RevenusMensuels = revenusMensuels,
                    Notifications = notifications ?? new List<Notification>(),
                    NbrClient = nbrlcient,
                    Abonnements = abonnementfournisseur
                }
            };


            // Retour de la vue avec le modèle
            return View("~/Views/AdminPage/IndexPage.cshtml", model);
        }


        // GET: AbonnementsController
        public ActionResult AbonnementPage(string? keyword = null, DateTime? DateDebut = null, DateTime? ExpirationDate = null, string? type = null, int? idcategorie = null, int pageNumber = 1)
        {
            int pageSize = 8; // Nombre d'abonnements par page
            List<Abonnement> abonnements;
            
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
                    abonnements = _abonnementService.FiltrePerDate(DateDebut.Value, ExpirationDate.Value);
                }
            }
            else if (!string.IsNullOrEmpty(type))
            {
                abonnements = _abonnementService.FiltrePerType(type );
            }
            else if (idcategorie.HasValue)
            {
                abonnements = _abonnementService.FiltrePerCategorie(idcategorie.Value);
            }
            else
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    abonnements = _abonnementService.getAbonnements(pageNumber, pageSize);
                }
                else
                {
                    abonnements = _abonnementService.searchMultiplyAbonnement(keyword);
                }
            }

            List<Fournisseur> fournisseurs = _fournisseurService.GetFournisseurs();
            List<Categorie> categories = _categorieService.GetCategories();
            List<Departement> departements = _departementService.getDepartements();

            int totalAbonnements = _abonnementService.CountTotalAbonnements();
            int totalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);

            var abonnementExpirant = _abonnementService.getAbonnementsExpiredOnMonthAdmin();

            var viewModel = new GlobalViewModel
            {
                AbonnementViewModel = new AbonnementViewModel
                {
                    Abonnements = abonnements,
                    Fournisseurs = fournisseurs,
                    Categories = categories,
                    CurrentPage = pageNumber,
                    TotalPages = totalPages,
                    Departements = departements,
                    TotalAbonnements = totalAbonnements,
                }
            };

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAbonnements = totalAbonnements;
            ViewBag.AbonnementExpirant = abonnementExpirant;

            return View("~/Views/AdminPage/abonnementPage.cshtml", viewModel);
        }


        // GET: AbonnementsController/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            var abonnementDetails = _abonnementService.GetAbonnementById(id);

            if (abonnementDetails == null)
            {
                Console.WriteLine("Aucun abonnement trouvé pour l'ID : " + id);
                return RedirectToAction("AbonnementPage");
            }

            Console.WriteLine("Abonnement trouvé : " + abonnementDetails.Nom);

            
            return View("~/Views/AdminPage/DetailsPage.cshtml" , abonnementDetails);
        }


        public IActionResult Test()
        {

            return View("~/Views/AdminPage/Test.cshtml");
        }

        // GET: AbonnementsController/Create
        public ActionResult Create()
        {
            

            List<Fournisseur> fournisseurs = _fournisseurService.GetFournisseurs();
            List<Categorie> categories = _categorieService.GetCategories();
            List<Departement> departements = _departementService.getDepartements();

            var fournisseursList = fournisseurs;
            var categoriesList = categories;
            var departementsList = departements;

            var viewModel = new GlobalViewModel
            {
                AbonnementViewModel = new AbonnementViewModel
                {
                    Fournisseurs = fournisseursList,
                    Categories = categoriesList,
                    Departements = departementsList
                }
            };
            
            return View("~/Views/AdminPage/CreateAbonnementPage.cshtml" , viewModel);
        }

        // POST: AbonnementsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Abonnement newAbonnement)
        {
            try
            {
                var abonnementToCreate = _abonnementService.SaveAbonnement(newAbonnement);
                
                if (abonnementToCreate != null)
                {
                    TempData["Message"] = "Abonnement créé avec succès";
                    return RedirectToAction("AbonnementPage");
                }
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors de la création de l'abonnement : " + ex.Message;
                return RedirectToAction("CreateAbonnementPage");
            }

            return View();
        }

        // GET: AbonnementsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View(id);
        }

        // POST: AbonnementsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Abonnement abonnementobj)
        {
            try
            {
                // Log des données reçues
                Console.WriteLine($"Abonnement reçu - ID: {abonnementobj.Id}, Nom: {abonnementobj.Nom}, Prix: {abonnementobj.Prix}");

                // Appel de la méthode de mise à jour
                string message = _abonnementService.updateAbonnement(abonnementobj);
                
                // Gestion du message de succès ou d'erreur
                TempData["Message"] = message;
                
                // Redirection vers la page des abonnements après la mise à jour
                return RedirectToAction("AbonnementPage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors de la mise à jour : " + ex.Message;
                return RedirectToAction("AbonnementPage");
            }
        }



        // GET: AbonnementsController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                // Appeler la méthode de suppression avec gestion des notifications
                string message = _abonnementService.deleteAbonnement(id);

                // Gérer le retour du message
                if (message == "Abonnement et ses notifications associées supprimés avec succès.")
                {
                    TempData["Message"] = message;
                    return RedirectToAction("AbonnementPage");
                }
                else if (message == "L'abonnement n'a pas été trouvé.")
                {
                    TempData["Error"] = "L'abonnement demandé n'existe pas.";
                    return RedirectToAction("AbonnementPage");
                }
                else
                {
                    TempData["Error"] = "Une erreur s'est produite lors de la suppression de l'abonnement.";
                    return RedirectToAction("AbonnementPage");
                }
            }
            catch (Exception ex)
            {
                // En cas d'exception, afficher un message d'erreur générique
                Console.WriteLine("Erreur lors de la suppression : " + ex.Message);
                TempData["Error"] = "Une erreur inattendue s'est produite.";
                return RedirectToAction("AbonnementPage");
            }
        }


        //Controllers pour le cote client


        public IActionResult HistoriquePage(int pageNumber = 1)
        {
            try
            {
                int pageSize = 6;

                // Vérifier si le service est initialisé
                if (_abonnementService == null)
                {
                    throw new Exception("Le service d'abonnement n'est pas disponible.");
                }

                // Récupération des abonnements
                List<VAbonnementClient> abonnement = _abonnementService.getListVAbonnement(pageNumber, pageSize) ?? new List<VAbonnementClient>();
                int nbrlcient = _abonnementService.NbrClientAbonne();
                Console.WriteLine("Nombre de clients abonnées : " + nbrlcient);

                int totalAbonnements = _abonnementService.CountTotalVAbonnement();
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);
                ViewBag.TotalAbonnements = totalAbonnements;
                ViewBag.NbrClientAbonne = nbrlcient;

                var viewModel = new GlobalViewModel
                {
                    HistoriqueViewModel = new HistoriqueViewModel
                    {
                        Abonnements = abonnement,
                        TotalAbonnements = totalAbonnements,
                        NbrClientAbonne = nbrlcient,
                    }
                };
            
                return View("~/Views/AdminPage/HistoriquePage.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite : " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult HistoriqueDetail(int pageNumberActifs = 1 , int pageNumberEnAttente = 1 , int pageNumberExpires = 1 , int pageNumberClients = 1)
        {
            try
            {
                int pageSize = 6;

                if (_abonnementService == null)
                {
                    throw new Exception("Le service d'abonnement n'est pas disponible.");
                }

                var (actifs , enAttente , expires) = _abonnementService.getListAbonnementByStatus(pageNumberActifs , pageNumberEnAttente , pageNumberExpires , pageSize);

                int totalActifs = _abonnementService.CountTotalAbonnementsActif();
                int totalEnAttente = _abonnementService.CountTotalAbonnementsEnAttente();
                int totalExpires = _abonnementService.CountTotalAbonnementsInactif();

                //Client abonnées////////////////////////////////////////////////////////
                List<VAbonnementClient> abonnement = _abonnementService.getListVAbonnement(pageNumberClients, pageSize) ?? new List<VAbonnementClient>();
                int nbrlcient = _abonnementService.NbrClientAbonne();
                Console.WriteLine("Nombre de clients abonnées : " + nbrlcient);
                int totalAbonnements = _abonnementService.CountTotalVAbonnement();

                ViewBag.CurrentPage = pageNumberClients;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);
                ViewBag.TotalAbonnements = totalAbonnements;
                ViewBag.NbrClientAbonne = nbrlcient;
                ////end client abonnées////////////////////////////////////////////////////////

                var viewModel = new GlobalViewModel
                {
                    HistoriqueViewModel = new HistoriqueViewModel
                    {
                        Actifs = actifs,
                        EnAttente = enAttente,
                        Expirés = expires,
                        CurrentPageActifs = pageNumberActifs,
                        CurrentPageEnAttente = pageNumberEnAttente,
                        CurrentPageExpirés = pageNumberExpires,
                        TotalActifs = totalActifs,
                        TotalEnAttente = totalEnAttente,
                        TotalExpires = totalExpires,
                        TotalPagesActifs = (int)Math.Ceiling((double)totalActifs / pageSize),
                        TotalPagesEnAttente = (int)Math.Ceiling((double)totalEnAttente / pageSize),
                        TotalPagesExpirés = (int)Math.Ceiling((double)totalExpires / pageSize),

                        ////Client abonnées////////////////////////////////////////////////////////
                        Abonnements = abonnement,
                        
                    }
                };

                Console.WriteLine("Total actifs : " + totalActifs);
                Console.WriteLine("Current page actifs : " + pageNumberActifs);
                Console.WriteLine("Total pages actifs : " + viewModel.HistoriqueViewModel.TotalPagesActifs);
                

                return View("~/Views/AdminPage/HistoriqueDetail.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite : " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult ExportToPdf(int id)
        {
            var abonnement = _abonnementService.GetAbonnementById(id);
            if (abonnement == null)
            {
                Console.WriteLine("Aucun abonnement trouvé pour l'ID : " + id);
                return NotFound();
            }

            return new ViewAsPdf("~/Views/AdminPage/PdfPage.cshtml", abonnement)
            {
                FileName = $"{abonnement.Nom}Details.pdf"
            };
            
        }
    }
}
