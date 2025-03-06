namespace suivi_abonnement.Service.Interface
{
    public interface IDepartementService
    {
        List<Departement> getDepartements();
        List<Departement> GetDepartementsByDirection(int idDirection);
    }
    
}