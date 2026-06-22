using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Common
{
    public class BackOfficePermissionAttribute : TypeFilterAttribute
    {
        public BackOfficePermissionAttribute(string moduleCode, bool isSuperAdminRequired = false, params string[] permissionCodes) : base(typeof(PermissionActionFilter))
        {
            Arguments = new object[] { moduleCode, isSuperAdminRequired, permissionCodes };
        }

        public BackOfficePermissionAttribute(string moduleCode, params string[] permissionCodes) : base(typeof(PermissionActionFilter))
        {
            Arguments = new object[] { moduleCode, false, permissionCodes };
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class PermissionActionFilter : BaseOfficePermissionAttribute
        {
            public PermissionActionFilter(string moduleCode,bool isSuperAdminRequired = false,params string[] permissionCodes) : base(moduleCode, isSuperAdminRequired, permissionCodes)
            {

            }
        }
    }

}
