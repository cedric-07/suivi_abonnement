using System.Collections.Generic;
using suivi_abonnement.Models;
namespace suivi_abonnement.Service.Interface
{
    public interface ICategorieService
    {
        List<Categorie> GetCategories();
        Categorie SaveCategorie(Categorie categorie);
    }
    
}