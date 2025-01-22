using suivi_abonnement.Service.Interface;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Models;
namespace suivi_abonnement.Service
{
    public class FournisseurService : IFournisseurService
    {
        private readonly IFournisseurRepository _fournisseurRepository;
        public FournisseurService(IFournisseurRepository fournisseurRepository)
        {
            _fournisseurRepository = fournisseurRepository;
        }
        public List<Fournisseur> GetFournisseursPaginate(int pageNumber, int pageSize)
        {
            return _fournisseurRepository.GetFournisseursPaginate(pageNumber, pageSize);
        }
        public int CountTotalOfFournisseurs()
        {
            return _fournisseurRepository.CountTotalOfFournisseurs();
        }
        public List<Fournisseur> GetFournisseurs()
        {
            return _fournisseurRepository.GetFournisseurs();
        }
        public Fournisseur SaveFournisseur(Fournisseur fournisseur)
        {
            return _fournisseurRepository.SaveFournisseur(fournisseur);
        }
        public Fournisseur UpdateFournisseur(Fournisseur fournisseur)
        {
            return _fournisseurRepository.UpdateFournisseur(fournisseur);
        }
        public void DeleteFournisseur(int id)
        {
            _fournisseurRepository.DeleteFournisseur(id);
        }
    }
}