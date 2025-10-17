namespace TravelSaaS.Models.DTOs
{
    // DTO pour créer une agence
    public class CreateAgencyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // DTO pour mettre à jour une agence
    public class UpdateAgencyDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // DTO pour créer un point d'agence
    public class CreateAgencyPointDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Guid AgencyId { get; set; }
    }

    // DTO pour mettre à jour un point d'agence
    public class UpdateAgencyPointDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // DTO pour créer un utilisateur administrateur
    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

       
        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty; // SuperAdmin, AgencyGlobalAdmin, AgencyPointAdmin, AgencyOperator
        public Guid? AgencyId { get; set; }
        public Guid? AgencyPointId { get; set; }
    }

    // DTO pour mettre à jour un utilisateur
    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public Guid? AgencyId { get; set; }
        public Guid? AgencyPointId { get; set; }
    }

    // DTO pour activer/désactiver un élément
    public class ToggleStatusDto
    {
        public bool IsActive { get; set; }
    }

    // DTO pour la réponse utilisateur
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public bool IsActive { get; set; }
        public bool IsSuperAdmin { get; set; }
        public Guid? AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public Guid? AgencyPointId { get; set; }
        public string? AgencyPointName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    // DTO pour la réponse agence
    public class AgencyResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UsersCount { get; set; }
        public int AgencyPointsCount { get; set; }
    }

    // DTO pour la réponse point d'agence
    public class AgencyPointResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UsersCount { get; set; }
    }
}
