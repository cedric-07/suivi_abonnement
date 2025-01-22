using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using System.Collections.Generic;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Repository.Interface;
namespace suivi_abonnement.Controllers
{
    public class AbonnementsController : Controller
    {
        private readonly IAbonnementService _abonnementService;
        private readonly IFournisseurService _fournisseurService;
        private readonly ICategorieService _categorieService;
        private readonly AbonnementViewModel abonnementViewModel = new AbonnementViewModel();
        private readonly AbonnementStatViewModel abonnementStatViewModel = new AbonnementStatViewModel();

        private readonly IDepartementService _departementService;
        private readonly User user = new User();
        public AbonnementsController(IAbonnementService abonnementService, IFournisseurService fournisseurService, ICategorieService categorieService, IDepartementService departementService)
        {
            _abonnementService = abonnementService;
            _fournisseurService = fournisseurService;
            _categorieService = categorieService;
            _departementService = departementService;
        }
        
        public IActionResult Index()
        {
            // Appel des méthodes pour obtenir les nombres d'abonnements
            int abonnementsActifs = _abonnementService.CountTotalAbonnementsActif();
            int abonnementsExpirés = _abonnementService.CountTotalAbonnementsInactif(); // Implémentez cette méthode
            int abonnementsSuspendus = _abonnementService.CountTotalAbonnementsEnAttente(); // Implémentez cette méthode
            List<Abonnement> listeAbonnementActif = _abonnementService.getListAbonnementActif();
            List<Abonnement> listeAbonnementExpiré = _abonnementService.getListAbonnementExpirer();
            List<Abonnement> listeAbonnementEnAttente = _abonnementService.getListAbonnementEnAttente();


            // Appel des méthodes pour obtenir les revenus fictifs
            List<Dictionary<string, object>> revenusAnnuels = _abonnementService.RevenusFictifsParAnnee();
            List<Dictionary<string, object>> revenusMensuels = _abonnementService.RevenusFictifsParMois();

            // Créer un objet pour les passer à la vue
            var model = new AbonnementStatViewModel
            {
                Actifs = abonnementsActifs,
                Expirés = abonnementsExpirés,
                Suspendus = abonnementsSuspendus,
                RevenusAnnuels = revenusAnnuels,
                RevenusMensuels = revenusMensuels,
                listeAbonnementActif = listeAbonnementActif,
                listeAbonnementExpiré = listeAbonnementExpiré,
                listeAbonnementEnAttente = listeAbonnementEnAttente
            };

            return View("~/Views/AdminPage/IndexPage.cshtml", model);
        }


        // GET: AbonnementsController
        public ActionResult AbonnementPage(string? keyword = null, DateTime? DateDebut = null, DateTime? ExpirationDate = null, string? type = null, int? idcategorie = null, int pageNumber = 1)
        {
            int pageSize = 6; // Nombre d'abonnements par page
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

            var viewModel = new AbonnementViewModel
            {
                Abonnements = abonnements,
                Fournisseurs = fournisseurs,
                Categories = categories,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                Departements = departements,
                TotalAbonnements = totalAbonnements
            };

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAbonnements = totalAbonnements;

            return View("~/Views/AdminPage/abonnementPage.cshtml", viewModel);
        }

   
        // GET: AbonnementsController/Details/5
        // public ActionResult Details(int id)
        // {
        //     Abonnement abonnementDetails = abonnement.GetAllAbonnements().Find(a => a.Id == id);
        //     if (abonnementDetails == null)
        //     {
        //         return NotFound();
        //     }
        //     return View(abonnementDetails);
        // }

        // GET: AbonnementsController/Create
        public ActionResult Create()
        {
            

            List<Fournisseur> fournisseurs = _fournisseurService.GetFournisseurs();
            List<Categorie> categories = _categorieService.GetCategories();
            List<Departement> departements = _departementService.getDepartements();

            var fournisseursList = fournisseurs;
            var categoriesList = categories;
            var departementsList = departements;

            var viewModel = new AbonnementViewModel
            {
                Fournisseurs = fournisseursList,
                Categories = categoriesList,
                Departements = departementsList

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
                int totalAbonnements = _abonnementService.CountTotalVAbonnement();

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalAbonnements / pageSize);
                ViewBag.TotalAbonnements = totalAbonnements;
                ViewBag.NbrClientAbonne = nbrlcient;
                Console.WriteLine("Total abonnements : " + totalAbonnements);
                Console.WriteLine("Current page : " + pageNumber);
                Console.WriteLine("Total pages : " + ViewBag.TotalPages);
                return View("~/Views/AdminPage/HistoriquePage.cshtml", abonnement);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite : " + ex.Message;
                return RedirectToAction("Index");
            }
        }




    }
}
