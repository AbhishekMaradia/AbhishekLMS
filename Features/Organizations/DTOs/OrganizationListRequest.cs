using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Organizations.DTOs
{
    public class OrganizationListRequest : BasePagedRequest
    {
        public bool? IsActive { get; set; }
    }
}
