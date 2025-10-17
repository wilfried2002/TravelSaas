using Microsoft.AspNetCore.Identity;


namespace TravelSaaS.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? AgencyId { get; set; }
        public Agency? Agency { get; set; }
        public Guid? AgencyPointId { get; set; }
        public AgencyPoint? AgencyPoint { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}
