using LMS_SoulCode.Features.Common.Models;

namespace LMS_SoulCode.Features.Course.Models
{
    public class Category : BaseTenantEntity
    {
        public string CategoryName { get; set; } = null!;
    }
}
