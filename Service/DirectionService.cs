using suivi_abonnement.Models;
using suivi_abonnement.Service.Interface;
using suivi_abonnement.Repository.Interface;
using System.Collections.Generic;
namespace suivi_abonnement.Service
{
    public class DirectionService:IDirectionService
    {
        private readonly IDirectionRepository _directionRepository;
        public DirectionService(IDirectionRepository directionRepository)
        {
            _directionRepository = directionRepository;
        }
        public List<Direction> GetDirections()
        {
            return _directionRepository.GetDirections();
        }
    }
}