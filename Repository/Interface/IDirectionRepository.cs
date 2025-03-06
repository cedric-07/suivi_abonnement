using suivi_abonnement.Models;
using System.Collections.Generic;
namespace suivi_abonnement.Repository.Interface
{
    public interface IDirectionRepository
    {
        List<Direction> GetDirections();
        
    }
}
