using System.Diagnostics;

namespace TravelSaaS.Models.Entities
{
    public class Agency
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Travel> Travels { get; set; }
    }
}
