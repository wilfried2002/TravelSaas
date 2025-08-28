using Microsoft.AspNetCore.Identity;


namespace TravelSaaS.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? AgencyId { get; set; }
        public Agency Agency { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
