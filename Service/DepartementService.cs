using suivi_abonnement.Models;
using suivi_abonnement.Repository.Interface;
using suivi_abonnement.Service.Interface;
using System.Collections.Generic;
using suivi_abonnement.Repository;

namespace suivi_abonnement.Service
{
    public class DepartementService : IDepartementService
    {
        private readonly IDepartementRepository _departementRepository;
        public DepartementService(IDepartementRepository departementRepository)
        {
            _departementRepository = departementRepository;
        }
        public List<Departement> getDepartements()
        {
            return _departementRepository.getDepartements();
        }
    }
}