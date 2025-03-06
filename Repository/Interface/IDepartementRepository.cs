namespace  suivi_abonnement.Repository.Interface
{
    public interface IDepartementRepository
    {
        List<Departement> getDepartements();
        List<Departement> GetDepartementsByDirection(int idDirection);
    }
}
