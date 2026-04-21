using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class PermissionListRequest : BasePagedRequest
    {
        public bool? IsActive { get; set; }
    }
}
