namespace TravelSaaS.Models.Entities
{
    public class Travel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AgencyId { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Agency Agency { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
