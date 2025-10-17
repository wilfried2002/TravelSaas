namespace TravelSaaS.Models.Entities
{
    public class AgencyPoint
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Travel> Travels { get; set; } = new List<Travel>();
    }
}
