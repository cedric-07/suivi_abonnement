using suivi_abonnement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace suivi_abonnement.Repository.Interface
{
    public interface IAbonnementRepository
    {
        List<Abonnement> getAbonnements(int pageNumber, int pageSize);
        int CountTotalAbonnements();
        Abonnement SaveAbonnement(Abonnement abonnement);
        List<Abonnement> GetAbonnementById(int id);
        string updateAbonnement(Abonnement abonnement);
        string deleteAbonnement(int id);
        List<Abonnement> searchMultiplyAbonnement(string keyword);
        List<Abonnement> FiltrePerDate(DateTime date_debut, DateTime expiration_date);
        List<Abonnement> FiltrePerCategorie(int idcategorie);
        List<Abonnement> FiltrePerType(string type);
        int CountTotalAbonnementsActif();
        int CountTotalAbonnementsInactif();
        int CountTotalAbonnementsEnAttente();
        List<Dictionary<string, object>> RevenusFictifsParAnnee();
        List<Dictionary<string, object>> RevenusFictifsParMois();
        List<Abonnement> getAbonnementByUser(int pageNumber , int pageSize , int userId);
        List<Abonnement> searchMultiplyMot(string keyword , int userId);
        List<Abonnement> FiltreDate(DateTime date_debut, DateTime expiration_date , int userId);
        List<Abonnement> FiltreCategorie(int idcategorie , int userId);
        List<Abonnement> FiltreType(string type , int userId);
        List<VAbonnementClient> getListVAbonnement(int pageNumber, int pageSize);
        int CountTotalVAbonnement();
        int NbrClientAbonne();
        (List<Abonnement> actifs, List<Abonnement> expires, List<Abonnement> enAttente) getListAbonnementStatus(int pageNumber, int pageSize);

    }
}