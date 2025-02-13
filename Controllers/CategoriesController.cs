using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
namespace suivi_abonnement.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategorieService _categorieService;
        private readonly INotificationService _notificationService;

        public CategoriesController(ICategorieService categorie , INotificationService notificationService)
        {
            this._categorieService = categorie;
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

        // POST: AbonnementsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Categorie categorie)
        {
            try
            {
                var categorieToCreate = _categorieService.SaveCategorie(categorie);    

                if (categorieToCreate != null)
                {
                    TempData["Message"] = "Catégorie créée avec succès";
                    // Rediriger vers l'action "Create" du contrôleur "Fournisseurs"
                    return View("~/Views/AdminPage/CreateFournisseurPage.cshtml");
                }
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Une erreur s'est produite lors de la création de la catégorie : " + ex.Message;
                // Rediriger vers l'action "Create" du contrôleur "Fournisseurs"
                return View("~/Views/AdminPage/CreateFournisseurPage.cshtml");
            }

            return View();
        }



        
    }
}
