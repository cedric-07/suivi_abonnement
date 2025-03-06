namespace suivi_abonnement.Models
{
    public class AbonnementViewModel
    {
        public int Id { get; set; } // Add this line

        public string? Nom { get; set; }

        public string? Description { get; set; }

        public decimal Prix { get; set; }

        public int Type { get; set; }

        public DateTime DateDebut { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string? NomCategorie { get; set; }

        public string? NomFournisseur { get; set; }

        public string? NomDepartement { get; set; }


        public int CurrentPage {get; set; }
        public int TotalPages {get; set; }

        public  int TotalAbonnements {get; set; }
        public List<Abonnement>? Abonnements { get; set; }
        public List<Direction>? Directions { get; set; }

        public Abonnement? Abonnement { get; set; }
        public IEnumerable<Fournisseur>? Fournisseurs { get; set; }
        public IEnumerable<Categorie>? Categories { get; set; }
        public List<Departement>? Departements { get; set; }

        public List<VAbonnementClient>? AbonnementClients { get; set; }

        public User? User { get; set; }
        public MessageViewModel? MessageViewModel {get; set; }
        
    }
}
