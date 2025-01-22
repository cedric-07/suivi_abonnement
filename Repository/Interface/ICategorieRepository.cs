using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface ICategorieRepository 
    {
        List<Categorie> GetCategories();
        Categorie SaveCategorie(Categorie categorie);
    }
}
