using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Service
{
    public class CategorieService : ICategorieService
    {
        private readonly ICategorieRepository _categorieRepository;
        public CategorieService(ICategorieRepository categorieRepository)
        {
            _categorieRepository = categorieRepository;
        }
        public List<Categorie> GetCategories()
        {
            return _categorieRepository.GetCategories();
        }
        public Categorie SaveCategorie(Categorie categorie)
        {
            return _categorieRepository.SaveCategorie(categorie);
        }
    }
    
}