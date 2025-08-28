using System.Diagnostics;

namespace TravelSaaS.Models.Entities
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TravelId { get; set; }
        public Guid ClientId { get; set; }
        public Guid AgencyId { get; set; }
        public int NumberOfPassengers { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmationDate { get; set; }

        // Navigation properties
        public Travel Travel { get; set; }
        public Client Client { get; set; }
        public Agency Agency { get; set; }
    }
}
