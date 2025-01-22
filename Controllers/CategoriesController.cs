using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using System.Collections.Generic;

namespace suivi_abonnement.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategorieService _categorieService;

        public CategoriesController(ICategorieService categorie)
        {
            this._categorieService = categorie;
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
