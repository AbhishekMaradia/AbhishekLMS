namespace LMS_SoulCode.Features.Common
{
    public static class PermissionCodes
    {
        // --- LEVEL 1: CORE DEFINITIONS (CRUD) ---
        
        // Roles
        public const string ROLE_VIEW = "ROLE_VIEW";
        public const string ROLE_ADD = "ROLE_ADD";
        public const string ROLE_EDIT = "ROLE_EDIT";
        public const string ROLE_DELETE = "ROLE_DELETE";

        // Modules
        public const string MODULE_VIEW = "MODULE_VIEW";
        public const string MODULE_ADD = "MODULE_ADD";
        public const string MODULE_EDIT = "MODULE_EDIT";
        public const string MODULE_DELETE = "MODULE_DELETE";

        // Permissions
        public const string PERMISSION_VIEW = "PERMISSION_VIEW";
        public const string PERMISSION_ADD = "PERMISSION_ADD";
        public const string PERMISSION_EDIT = "PERMISSION_EDIT";
        public const string PERMISSION_DELETE = "PERMISSION_DELETE";

        // --- LEVEL 2: SCHEMA MAPPINGS (What Perms belong to what Module) ---
        
        public const string MODULE_PERMISSION_VIEW = "MODULE_PERMISSION_VIEW";
        public const string MODULE_PERMISSION_ASSIGN = "MODULE_PERMISSION_ASSIGN";
        public const string MODULE_PERMISSION_DELETE = "MODULE_PERMISSION_DELETE";

        // --- LEVEL 3: ROLE ALLOTMENTS & MATRIX (Who gets what Module/Perm) ---
        
        // Role-Module
        public const string ROLE_MODULE_VIEW = "ROLE_MODULE_VIEW";
        public const string ROLE_MODULE_ASSIGN = "ROLE_MODULE_ASSIGN";
        public const string ROLE_MODULE_DELETE = "ROLE_MODULE_DELETE";

        // Role-Module-Permission Matrix
        public const string ROLE_MODULE_PERMISSION_VIEW = "ROLE_MODULE_PERMISSION_VIEW";
        public const string ROLE_MODULE_PERMISSION_ASSIGN = "ROLE_MODULE_PERMISSION_ASSIGN";
        public const string ROLE_MODULE_PERMISSION_DELETE = "ROLE_MODULE_PERMISSION_DELETE";

        // --- LEVEL 4: USER ROLE ASSIGNMENTS ---
        
        public const string USER_ROLE_VIEW = "USER_ROLE_VIEW";
        public const string USER_ROLE_ASSIGN = "USER_ROLE_ASSIGN";
        public const string USER_ROLE_DELETE = "USER_ROLE_DELETE";

        // --- OTHER APPLICATION MODULES ---
        
        // Course
        public const string COURSE_VIEW = "COURSE_VIEW";
        public const string COURSE_ADD = "COURSE_ADD";
        public const string COURSE_EDIT = "COURSE_EDIT";
        public const string COURSE_DELETE = "COURSE_DELETE";

        // Category
        public const string CATEGORY_VIEW = "CATEGORY_VIEW";
        public const string CATEGORY_ADD = "CATEGORY_ADD";
        public const string CATEGORY_EDIT = "CATEGORY_EDIT";
        public const string CATEGORY_DELETE = "CATEGORY_DELETE";

        // User (General)
        public const string USER_VIEW = "USER_VIEW";
        public const string USER_ADD = "USER_ADD";
        public const string USER_EDIT = "USER_EDIT";
        public const string USER_DELETE = "USER_DELETE";

        // Video
        public const string VIDEO_VIEW = "VIDEO_VIEW";
        public const string VIDEO_ADD = "VIDEO_ADD";
        public const string VIDEO_EDIT = "VIDEO_EDIT";
        public const string VIDEO_DELETE = "VIDEO_DELETE";

        // Organization
        public const string ORGANIZATION_VIEW = "ORGANIZATION_VIEW";
        public const string ORGANIZATION_ADD = "ORGANIZATION_ADD";
        public const string ORGANIZATION_EDIT = "ORGANIZATION_EDIT";
        public const string ORGANIZATION_DELETE = "ORGANIZATION_DELETE";

        // Group
        public const string GROUP_VIEW = "GROUP_VIEW";
        public const string GROUP_ADD = "GROUP_ADD";
        public const string GROUP_EDIT = "GROUP_EDIT";
        public const string GROUP_DELETE = "GROUP_DELETE";

        // Certificate
        public const string CERTIFICATE_VIEW = "CERTIFICATE_VIEW";
        public const string CERTIFICATE_ADD = "CERTIFICATE_ADD";
        public const string CERTIFICATE_EDIT = "CERTIFICATE_EDIT";
        public const string CERTIFICATE_DELETE = "CERTIFICATE_DELETE";

        // Subscription
        public const string SUBSCRIPTION_VIEW = "SUBSCRIPTION_VIEW";
        public const string SUBSCRIPTION_ADD = "SUBSCRIPTION_ADD";
        public const string SUBSCRIPTION_EDIT = "SUBSCRIPTION_EDIT";
        public const string SUBSCRIPTION_DELETE = "SUBSCRIPTION_DELETE";

        // Reports
        public const string REPORT_VIEW = "REPORT_VIEW";
        public const string REPORT_GENERATE = "REPORT_GENERATE";

        // File Management
        public const string FILE_ENCRYPT = "FILE_ENCRYPT";
        public const string FILE_DECRYPT = "FILE_DECRYPT";
        public const string FILE_UPLOAD = "FILE_UPLOAD";
        public const string FILE_DOWNLOAD = "FILE_DOWNLOAD";
    }
}
