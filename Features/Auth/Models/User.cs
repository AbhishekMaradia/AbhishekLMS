using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Organizations.Models;
using LMS_SoulCode.Features.Groups.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_SoulCode.Features.Auth.Models
{
    public class User : BaseTenantEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Mobile { get; set; } = null!;        
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ResetToken { get; set; }  
        public DateTime? ResetTokenExpiry { get; set; } 
        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiry { get; set; } 
        public bool IsActive { get; set; } = true;
        
        // Group Assignment (optional - organization will assign users to groups)
        public int? GroupId { get; set; }
        
        [ForeignKey("TenantId")]
        public Organization? Organization { get; set; } // Navigation Property
        
        [ForeignKey("GroupId")]
        public Group? Group { get; set; } // Navigation Property
    }
}
