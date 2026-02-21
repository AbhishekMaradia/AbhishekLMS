using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.UserPermissions.DTOs
{
    public class RoleModuleListRequest : BasePagedRequest
    {
        public int? RoleId { get; set; }
        public int? ModuleId { get; set; }
    }
}