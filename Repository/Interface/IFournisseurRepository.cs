using suivi_abonnement.Models;
namespace suivi_abonnement.Repository.Interface
{
    public interface IFournisseurRepository
    {
        List<Fournisseur> GetFournisseursPaginate(int pageNumber , int pageSize);
        int CountTotalOfFournisseurs();
        List<Fournisseur> GetFournisseurs();
        Fournisseur SaveFournisseur(Fournisseur fournisseur);
        Fournisseur UpdateFournisseur(Fournisseur fournisseur);
        void DeleteFournisseur(int id);
    }
}