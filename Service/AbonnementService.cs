using suivi_abonnement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
namespace suivi_abonnement.Service
{
    public class AbonnementService : IAbonnementService
    {
        private readonly IAbonnementRepository _abonnementRepository;
        public AbonnementService(IAbonnementRepository abonnementRepository)
        {
            _abonnementRepository = abonnementRepository;
        }
        public List<Abonnement> getAbonnements(int pageNumber, int pageSize)
        {
            return _abonnementRepository.getAbonnements(pageNumber, pageSize);
        }
        public int CountTotalAbonnements()
        {
            return _abonnementRepository.CountTotalAbonnements();
        }
        public Abonnement SaveAbonnement(Abonnement abonnement)
        {
            return _abonnementRepository.SaveAbonnement(abonnement) ;
        }
        public List<Abonnement> GetAbonnementById(int id)
        {
            return _abonnementRepository.GetAbonnementById(id);
        }
        public string updateAbonnement(Abonnement abonnement)
        {
            return _abonnementRepository.updateAbonnement(abonnement);
        }
        public string deleteAbonnement(int id)
        {
            return _abonnementRepository.deleteAbonnement(id);
        }
        public List<Abonnement> searchMultiplyAbonnement(string keyword)
        {
            return _abonnementRepository.searchMultiplyAbonnement(keyword);
        }
        public List<Abonnement> FiltrePerDate(DateTime date_debut, DateTime expiration_date)
        {
            return _abonnementRepository.FiltrePerDate(date_debut, expiration_date);
        }
        public List<Abonnement> FiltrePerCategorie(int idcategorie)
        {
            return _abonnementRepository.FiltrePerCategorie(idcategorie);
        }
        public List<Abonnement> FiltrePerType(string type)
        {
            return _abonnementRepository.FiltrePerType(type);
        }
        public int CountTotalAbonnementsActif()
        {
            return _abonnementRepository.CountTotalAbonnementsActif();
        }
        public int CountTotalAbonnementsInactif()
        {
            return _abonnementRepository.CountTotalAbonnementsInactif();
        }
        public int CountTotalAbonnementsEnAttente()
        {
            return _abonnementRepository.CountTotalAbonnementsEnAttente();
        }
        public List<Dictionary<string, object>> RevenusFictifsParAnnee()
        {
            return _abonnementRepository.RevenusFictifsParAnnee();
        }
        public List<Dictionary<string, object>> RevenusFictifsParMois()
        {
            return _abonnementRepository.RevenusFictifsParMois();
        }
        
        public List<Abonnement> getAbonnementByUser(int pageNumber, int pageSize, int userId)
        {
            return _abonnementRepository.getAbonnementByUser(pageNumber, pageSize, userId);
        }
        public List<Abonnement> searchMultiplyMot(string keyword, int userId)
        {
            return _abonnementRepository.searchMultiplyMot(keyword, userId);
        }
        public List<Abonnement> FiltreDate(DateTime date_debut, DateTime expiration_date, int userId)
        {
            return _abonnementRepository.FiltreDate(date_debut, expiration_date, userId);
        }
        public List<Abonnement> FiltreCategorie(int idcategorie, int userId)
        {
            return _abonnementRepository.FiltreCategorie(idcategorie, userId);
        }
        public List<Abonnement> FiltreType(string type, int userId)
        {
            return _abonnementRepository.FiltreType(type, userId);
        }

        public List<VAbonnementClient> getListVAbonnement(int pageNumber, int pageSize)
        {
            return _abonnementRepository.getListVAbonnement(pageNumber, pageSize);
        }
        public int CountTotalVAbonnement()
        {
            return _abonnementRepository.CountTotalVAbonnement();
        }
        public int NbrClientAbonne()
        {
            return _abonnementRepository.NbrClientAbonne();
        }
        public  (List<VStatusAbonnement> actifs, List<VStatusAbonnement> enAttente, List<VStatusAbonnement> expires) getListAbonnementByStatus(int pageNumberActifs , int pageNumberEnAttente, int pageNumberExpires, int pageSize)
        {
            return _abonnementRepository.getListAbonnementByStatus(pageNumberActifs, pageNumberEnAttente, pageNumberExpires, pageSize);
        }

        public List<Abonnement> getAbonnementsExpiredOnMonth()
        {
            return _abonnementRepository.getAbonnementsExpiredOnMonth();
        }
    }
}