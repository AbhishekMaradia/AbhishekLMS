using LMS_SoulCode.Features.Common.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_SoulCode.Features.Organizations.Models
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!; // Unique code (e.g., 'soulcode', 'org1')
        public string? LogoUrl { get; set; }
        public string? LogoThumbUrl { get; set; }
        [NotMapped]
        public IFormFile? Logo { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
