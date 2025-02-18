using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using System.Collections.Generic;
using suivi_abonnement.Service.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
namespace suivi_abonnement.Controllers
{
    public class FournisseursController : Controller
    {
        private readonly IFournisseurService _fournisseurService;
        private readonly INotificationService _notificationService;
        public FournisseursController(IFournisseurService fournisseurService , INotificationService notificationService)
        {
            this._fournisseurService = fournisseurService;
            this._notificationService = notificationService;
        }
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


        
        // GET: FournisseursController
        public ActionResult Index(int pageNumber = 1)
        {
            int pageSize = 5;  // Nombre d'éléments par page
            var fournisseurs = _fournisseurService.GetFournisseursPaginate(pageNumber, pageSize);
            int totalFournisseurs = _fournisseurService.CountTotalOfFournisseurs();

            // Debugging: afficher totalFournisseurs
            Console.WriteLine($"Total Fournisseurs: {totalFournisseurs}");

            // Vérification si totalFournisseurs est supérieur à zéro avant de calculer TotalPages
            ViewBag.TotalPages = totalFournisseurs > 0
                ? (int)Math.Ceiling((double)totalFournisseurs / pageSize)
                : 0;

            ViewBag.CurrentPage = pageNumber;
            ViewBag.totalFournisseurs = totalFournisseurs;

            var viewModel = new GlobalViewModel
            {
                FournisseurViewModel = new FournisseurViewModel
                {
                    Fournisseurs = fournisseurs,
                    CurrentPage = pageNumber,
                    TotalPages = ViewBag.TotalPages
                }
            };

            return View("~/Views/AdminPage/IndexFournisseurPage.cshtml", viewModel);
        }





        // GET: AbonnementsController/Details/5
        public ActionResult Details(int id)
        {
 
            return View();
        }

        // GET: AbonnementsController/Create
        public ActionResult Create()
        {
            return View("~/Views/AdminPage/CreateFournisseurPage.cshtml");
        }

        // POST: AbonnementsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Fournisseur fournisseur)
        {
            try
            {
                var fournisseurModel = new Fournisseur();
                var fournisseurToCreate = _fournisseurService.SaveFournisseur(fournisseur);    

                if (fournisseurToCreate != null)
                {
                    TempData["Message"] = "Fournisseur créé avec succès";
                    // Redirection vers l'action "CreateFournisseurPage" du contrôleur "AdminPage"
                    return RedirectToAction("Index", "Fournisseurs");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors de la création du fournisseur : " + ex.Message;
                // Redirection vers l'action "CreateFournisseurPage" du contrôleur "AdminPage"
                return RedirectToAction("Create", "Fournisseurs");
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
        public ActionResult Edit(Fournisseur fournisseur)
        {
            try
            {
                // Log des données reçues
                Console.WriteLine($"Fournisseur reçu - ID: {fournisseur.Id}, Nom: {fournisseur.Nom}, Prix: {fournisseur.Email} , Telephone: {fournisseur.Telephone}");

                // Appel de la méthode de mise à jour
                var fournisseurObject = new Fournisseur();
                var fournisseurToEdit = _fournisseurService.UpdateFournisseur(fournisseur);
                
                if (fournisseurToEdit != null)
                {
                    TempData["Message"] = "Fournisseur mis à jour avec succès";
                    return RedirectToAction("Index");
                }
                
                // Redirection vers la page des abonnements après la mise à jour
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors de la mise à jour : " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        // GET: AbonnementsController/Delete/5
        public ActionResult Delete(int id)
        {
            _fournisseurService.DeleteFournisseur(id);
            return RedirectToAction("Index");
        }

    }
}
