using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
namespace suivi_abonnement.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly ICategorieService _categorieService;
        private readonly INotificationService _notificationService;

        public CategoriesController(ICategorieService categorie , INotificationService notificationService): base(notificationService)
        {
            this._categorieService = categorie;
            this._notificationService = notificationService;
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
