namespace suivi_abonnement.Models
{
    public class FournisseurViewModel
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public List<Fournisseur>? Fournisseurs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    
}