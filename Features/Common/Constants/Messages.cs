namespace LMS_SoulCode.Features.Common
{
    public class Messages
    {
        public const string Success = "Success";
        public const string Created = "Created successfully";
        public const string Updated = "Updated successfully";
        public const string Deleted = "Deleted successfully";
        public const string Uploaded = "File uploaded successfully";

        public const string InvalidCredentials = "Invalid email or password";
        public const string AlreadyExists = "User already exists";
        public const string NotFound = "Not found";
        public const string Unauthorized = "Unauthorized";
        public const string BadRequest = "Bad request";
        public const string Forbidden = "You do not have permission to access this resource";

        public const string Error = "Something went wrong";
        public const string TokenGenerated = "Reset token generated";
        public const string PasswordUpdated = "Password updated successfully";
        public const string OrgAlreadyExists = "Organization code already exists";
        
        // Auth Specific
        public const string LoginSuccess = "Login successful";
        public const string OrgInactive = "Your organization is inactive or not registered. Please contact support.";
        public const string UserInactive = "Your account is inactive. Please contact your administrator.";
        public const string EmailExists = "Email already exists";
        public const string MobileExists = "Mobile number already exists";
        public const string EmailNotFound = "Email not found";
        public const string TokenCreationError = "Unable to create reset token";
        public const string InvalidToken = "Invalid or expired token";
        // User Permissions
        public const string UserNotFound = "User not found";
        public const string RoleNotFound = "Role not found or inactive";
        public const string ModuleNotFound = "Module not found or inactive";
        public const string UserRoleExists = "User already has this role assigned";
        public const string RoleAssigned = "Role assigned to user successfully";
        public const string NoValidPermissions = "No valid active permissions found";
        public const string NoPermissionsAssigned = "User has no permissions assigned";
        public const string UserRoleNotFound = "User does not have this role assigned";
        public const string RoleRemoved = "Role removed from user successfully";
        public const string NoRolesAssigned = "User has no roles assigned";
        public const string SystemRoleDeleteError = "Cannot delete System Admin Role";
        public const string PermissionsLinked = "Permissions linked to module successfully";
        public const string NoCoursesInCategory = "No courses found for this category";
        public const string NoFileSelected = "No file selected";
        public const string DeleteBlockedCategory = "Cannot delete this category because there are courses assigned to it.";
        public const string DeleteBlockedOrgGroups = "Cannot delete organization because it has active groups. Please remove groups first.";
        public const string DeleteBlockedOrgUsers = "Cannot delete organization because it has active users. Please remove users first.";
        public const string DeleteBlockedOrgCourses = "Cannot delete organization because it has active courses. Please remove courses first.";
        public const string DeleteBlockedOrgCategories = "Cannot delete organization because it has active categories. Please remove categories first.";
        public const string DeleteBlockedGroup = "Cannot delete this group because there are users currently assigned to it.";
        public const string DeleteBlockedRole = "Cannot delete this role because there are users currently assigned to it.";
        public const string DeleteBlockedCourseGroups = "This course is assigned to one or more groups. Please remove it from all groups before making this change.";
        public const string DeleteBlockedCourseUsers = "Cannot delete this course because students are already enrolled.";
        public const string UpdateBlockedMove = "Cannot change organization because this entity has active dependencies.";
        public const string DeleteBlockedUserAdmin = "Cannot delete this user because they are the Organization Admin. Please assign another admin first.";
        public const string SelfDeactivationError = "You cannot deactivate your own account.";
        public const string SelfDeletionError = "You cannot delete your own account.";
    }
}
