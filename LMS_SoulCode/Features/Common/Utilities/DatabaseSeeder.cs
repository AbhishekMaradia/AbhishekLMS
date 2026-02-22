// using LMS_SoulCode.Data;
// using LMS_SoulCode.Features.Auth.Models;
// using LMS_SoulCode.Features.Organizations.Models;
// using LMS_SoulCode.Features.UserPermissions.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Reflection; 
// using AppModule = LMS_SoulCode.Features.UserPermissions.Models.Module; // Alias to prevent ambiguity

// namespace LMS_SoulCode.Features.Common
// {
//     public class DatabaseSeeder
//     {
//         private readonly LmsDbContext _context;

//         public DatabaseSeeder(LmsDbContext context)
//         {
//             _context = context;
//         }

//         public async Task SeedAsync()
//         {
//             var strategy = _context.Database.CreateExecutionStrategy();

//             await strategy.ExecuteAsync(async () =>
//             {
//                 using var transaction = await _context.Database.BeginTransactionAsync();
//                 try
//                 {
//                     // Check if database is already seeded
//                     if (await _context.Roles.AnyAsync())
//                     {
//                         return;
//                     }

//                     // 1. Truncate/Delete Data (Disable Constraints for clean wipe)
//                     // We will NOT delete the Super Admin user, but we will delete their UserRoles to re-assign clean ones.
                    
//                     // Identify Super Admin
//                     // Query UserRoles directly since User doesn't have the navigation property
//                     var superAdminId = await _context.UserRoles
//                         .Where(ur => ur.Role.Code == "SUPER_ADMIN")
//                         .Select(ur => ur.UserId)
//                         .FirstOrDefaultAsync();

//                     User? superAdminUser = null;
//                     if (superAdminId != 0)
//                     {
//                         superAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == superAdminId);
//                     }
//                     if (superAdminUser == null)
//                     {
//                         superAdminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@gmail.com");
//                     }

//                     int? superAdminUserId = superAdminUser?.Id;
                    
//                     // If no super admin found, we might be wiping everything. That's fine, we'll recreate.

//                     // Delete dependent tables first
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM UserVideoProgresses");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM UserCourses");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM RoleModulePermissions");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM RoleModules");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM ModulePermissions");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM UserRoles");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Roles");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Modules");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Permissions");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM CourseVideos");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM CourseDocuments");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Courses");
//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Categories");
                    
//                     if (superAdminUserId.HasValue)
//                     {
//                         await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE Id != {0}", superAdminUserId.Value);
//                     }
//                     else
//                     {
//                         await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
//                     }

//                     await _context.Database.ExecuteSqlRawAsync("DELETE FROM Organizations");

//                     // 2. Insert Permissions (Granular from PermissionCodes.cs)
//                     // We need to ensure these match PermissionCodes.cs
//                     var permissions = new List<Permission>
//                     {
//                         // Course
//                         new() { Name = "View Course", Code = "COURSE_VIEW", IsActive = true },
//                         new() { Name = "Add Course", Code = "COURSE_ADD", IsActive = true },
//                         new() { Name = "Edit Course", Code = "COURSE_EDIT", IsActive = true },
//                         new() { Name = "Delete Course", Code = "COURSE_DELETE", IsActive = true },

//                         // Category
//                         new() { Name = "View Category", Code = "CATEGORY_VIEW", IsActive = true },
//                         new() { Name = "Add Category", Code = "CATEGORY_ADD", IsActive = true },
//                         new() { Name = "Edit Category", Code = "CATEGORY_EDIT", IsActive = true },
//                         new() { Name = "Delete Category", Code = "CATEGORY_DELETE", IsActive = true },

//                         // User
//                         new() { Name = "View User", Code = "USER_VIEW", IsActive = true },
//                         new() { Name = "Add User", Code = "USER_ADD", IsActive = true },
//                         new() { Name = "Edit User", Code = "USER_EDIT", IsActive = true },
//                         new() { Name = "Delete User", Code = "USER_DELETE", IsActive = true },

//                         // Video
//                         new() { Name = "View Video", Code = "VIDEO_VIEW", IsActive = true },
//                         new() { Name = "Add Video", Code = "VIDEO_ADD", IsActive = true },
//                         new() { Name = "Edit Video", Code = "VIDEO_EDIT", IsActive = true },
//                         new() { Name = "Delete Video", Code = "VIDEO_DELETE", IsActive = true },

//                         // Role
//                         new() { Name = "View Role", Code = "ROLE_VIEW", IsActive = true },
//                         new() { Name = "Add Role", Code = "ROLE_ADD", IsActive = true },
//                         new() { Name = "Edit Role", Code = "ROLE_EDIT", IsActive = true },
//                         new() { Name = "Delete Role", Code = "ROLE_DELETE", IsActive = true },

//                         // Permission Mgmt
//                         new() { Name = "View Permission", Code = "PERMISSION_VIEW", IsActive = true },
//                         new() { Name = "Add Permission", Code = "PERMISSION_ADD", IsActive = true },
//                         new() { Name = "Edit Permission", Code = "PERMISSION_EDIT", IsActive = true },
//                         new() { Name = "Delete Permission", Code = "PERMISSION_DELETE", IsActive = true },

//                         // Module Mgmt
//                         new() { Name = "View Module", Code = "MODULE_VIEW", IsActive = true },
//                         new() { Name = "Add Module", Code = "MODULE_ADD", IsActive = true },
//                         new() { Name = "Edit Module", Code = "MODULE_EDIT", IsActive = true },
//                         new() { Name = "Delete Module", Code = "MODULE_DELETE", IsActive = true },

//                         // Role-Module
//                         new() { Name = "View Role Module", Code = "ROLE_MODULE_VIEW", IsActive = true },
//                         new() { Name = "Add Role Module", Code = "ROLE_MODULE_ADD", IsActive = true },
//                         new() { Name = "Edit Role Module", Code = "ROLE_MODULE_EDIT", IsActive = true },
//                         new() { Name = "Delete Role Module", Code = "ROLE_MODULE_DELETE", IsActive = true },

//                         // User Permissions
//                         new() { Name = "View User Permission", Code = "USER_PERMISSION_VIEW", IsActive = true },
//                         new() { Name = "Assign User Permission", Code = "USER_PERMISSION_ASSIGN", IsActive = true },
//                         new() { Name = "Assign Role", Code = "ROLE_ASSIGN", IsActive = true },

//                         // Certificate
//                         new() { Name = "View Certificate", Code = "CERTIFICATE_VIEW", IsActive = true },
//                         new() { Name = "Add Certificate", Code = "CERTIFICATE_ADD", IsActive = true },
//                         new() { Name = "Edit Certificate", Code = "CERTIFICATE_EDIT", IsActive = true },
//                         new() { Name = "Delete Certificate", Code = "CERTIFICATE_DELETE", IsActive = true },

//                         // Subscription
//                         new() { Name = "View Subscription", Code = "SUBSCRIPTION_VIEW", IsActive = true },
//                         new() { Name = "Add Subscription", Code = "SUBSCRIPTION_ADD", IsActive = true },
//                         new() { Name = "Edit Subscription", Code = "SUBSCRIPTION_EDIT", IsActive = true },
//                         new() { Name = "Delete Subscription", Code = "SUBSCRIPTION_DELETE", IsActive = true },

//                         // Reports
//                         new() { Name = "View Report", Code = "REPORT_VIEW", IsActive = true },
//                         new() { Name = "Generate Report", Code = "REPORT_GENERATE", IsActive = true },

//                         // File Mgmt
//                         new() { Name = "Encrypt File", Code = "FILE_ENCRYPT", IsActive = true },
//                         new() { Name = "Decrypt File", Code = "FILE_DECRYPT", IsActive = true },
//                         new() { Name = "Upload File", Code = "FILE_UPLOAD", IsActive = true },
//                         new() { Name = "Download File", Code = "FILE_DOWNLOAD", IsActive = true },

//                         // Organization
//                         new() { Name = "View Organization", Code = "ORGANIZATION_VIEW", IsActive = true },
//                         new() { Name = "Add Organization", Code = "ORGANIZATION_ADD", IsActive = true },
//                         new() { Name = "Edit Organization", Code = "ORGANIZATION_EDIT", IsActive = true },
//                         new() { Name = "Delete Organization", Code = "ORGANIZATION_DELETE", IsActive = true },
                        
//                         // Group
//                         new() { Name = "View Group", Code = "GROUP_VIEW", IsActive = true },
//                         new() { Name = "Add Group", Code = "GROUP_ADD", IsActive = true },
//                         new() { Name = "Edit Group", Code = "GROUP_EDIT", IsActive = true },
//                         new() { Name = "Delete Group", Code = "GROUP_DELETE", IsActive = true }
//                     };
//                     await _context.Permissions.AddRangeAsync(permissions);
//                     await _context.SaveChangesAsync();

//                     // 3. Insert Modules
//                     // Explicitly use AppModule
//                     var modules = new List<AppModule>
//                     {
//                         new() { Name = "Authentication", Code = "AUTH", IsActive = true },
//                         new() { Name = "Dashboard", Code = "DASHBOARD", IsActive = true },
//                         new() { Name = "User Management", Code = "USER_MANAGEMENT", IsActive = true },
//                         new() { Name = "Role Management", Code = "ROLE_MANAGEMENT", IsActive = true },
//                         new() { Name = "Permission Management", Code = "PERMISSION_MANAGEMENT", IsActive = true },
//                         new() { Name = "Organization Management", Code = "ORGANIZATION_MANAGEMENT", IsActive = true },
//                         new() { Name = "Course Management", Code = "COURSE_MANAGEMENT", IsActive = true },
//                         new() { Name = "Video Management", Code = "VIDEO_MANAGEMENT", IsActive = true },
//                         new() { Name = "Reports", Code = "REPORTS", IsActive = true },
//                         new() { Name = "Group Management", Code = "GROUP", IsActive = true },
//                         new() { Name = "Settings", Code = "SETTINGS", IsActive = true },
//                         new() { Name = "Category Management", Code = "CATEGORY", IsActive = true }, // Added missing Category module
//                         new() { Name = "Certificate Management", Code = "CERTIFICATE", IsActive = true },
//                         new() { Name = "Subscription Management", Code = "SUBSCRIPTION", IsActive = true },
//                         new() { Name = "File Management", Code = "FILE_MANAGEMENT", IsActive = true }
//                     };
//                     await _context.Modules.AddRangeAsync(modules);
//                     await _context.SaveChangesAsync();

//                     // 4. Map Module Permissions
//                     // We must map specific permissions to specific modules
//                     var modulePermissions = new List<ModulePermission>();
                    
//                     // Helper to link
//                     void Link(string moduleCode, params string[] permCodes)
//                     {
//                         var mod = modules.FirstOrDefault(m => m.Code == moduleCode);
//                         if (mod == null) return;
                        
//                         foreach (var pCode in permCodes)
//                         {
//                             var perm = permissions.FirstOrDefault(p => p.Code == pCode);
//                             if (perm != null)
//                             {
//                                 modulePermissions.Add(new ModulePermission { ModuleId = mod.Id, PermissionId = perm.Id });
//                             }
//                         }
//                     }

//                     Link("COURSE_MANAGEMENT", "COURSE_VIEW", "COURSE_ADD", "COURSE_EDIT", "COURSE_DELETE");
//                     Link("CATEGORY", "CATEGORY_VIEW", "CATEGORY_ADD", "CATEGORY_EDIT", "CATEGORY_DELETE");
//                     Link("USER_MANAGEMENT", "USER_VIEW", "USER_ADD", "USER_EDIT", "USER_DELETE");
//                     Link("VIDEO_MANAGEMENT", "VIDEO_VIEW", "VIDEO_ADD", "VIDEO_EDIT", "VIDEO_DELETE");
//                     Link("ROLE_MANAGEMENT", "ROLE_VIEW", "ROLE_ADD", "ROLE_EDIT", "ROLE_DELETE");
//                     Link("PERMISSION_MANAGEMENT", "PERMISSION_VIEW", "PERMISSION_ADD", "PERMISSION_EDIT", "PERMISSION_DELETE");
//                     Link("USER_MANAGEMENT", "USER_PERMISSION_VIEW", "USER_PERMISSION_ASSIGN", "ROLE_ASSIGN"); // Assigning these to User Management for now
//                     // Link("MODULE_MANAGEMENT", "MODULE_VIEW", "MODULE_ADD", "MODULE_EDIT", "MODULE_DELETE"); // If Module management module exists
//                     Link("CERTIFICATE", "CERTIFICATE_VIEW", "CERTIFICATE_ADD", "CERTIFICATE_EDIT", "CERTIFICATE_DELETE");
//                     Link("SUBSCRIPTION", "SUBSCRIPTION_VIEW", "SUBSCRIPTION_ADD", "SUBSCRIPTION_EDIT", "SUBSCRIPTION_DELETE");
//                     Link("REPORTS", "REPORT_VIEW", "REPORT_GENERATE");
//                     Link("FILE_MANAGEMENT", "FILE_ENCRYPT", "FILE_DECRYPT", "FILE_UPLOAD", "FILE_DOWNLOAD");
//                     Link("ORGANIZATION_MANAGEMENT", "ORGANIZATION_VIEW", "ORGANIZATION_ADD", "ORGANIZATION_EDIT", "ORGANIZATION_DELETE");
//                     Link("GROUP", "GROUP_VIEW", "GROUP_ADD", "GROUP_EDIT", "GROUP_DELETE");

//                     await _context.ModulePermissions.AddRangeAsync(modulePermissions);
//                     await _context.SaveChangesAsync();

//                     // 5. Insert Roles
//                     var superAdminRole = new Role { Name = "Super Admin", Code = "SUPER_ADMIN", IsActive = true, TenantId = null };
//                     var orgAdminRole = new Role { Name = "Organization Admin", Code = "ORGANIZATION_ADMIN", IsActive = true, TenantId = null };
//                     var instructorRole = new Role { Name = "Instructor", Code = "INSTRUCTOR", IsActive = true, TenantId = null };
//                     var studentRole = new Role { Name = "Student", Code = "STUDENT", IsActive = true, TenantId = null };

//                     await _context.Roles.AddRangeAsync(superAdminRole, orgAdminRole, instructorRole, studentRole);
//                     await _context.SaveChangesAsync();

//                     // 6. Role-Module Mappings & Permissions
//                     var rolesToModules = new Dictionary<Role, List<AppModule>>
//                     {
//                         { superAdminRole, modules }, 
//                         { orgAdminRole, modules }, 
//                         { instructorRole, modules.Where(m => m.Code == "COURSE_MANAGEMENT" || m.Code == "VIDEO_MANAGEMENT" || m.Code == "DASHBOARD").ToList() },
//                         { studentRole, modules.Where(m => m.Code == "COURSE_MANAGEMENT" || m.Code == "DASHBOARD" || m.Code == "CERTIFICATE" || m.Code == "SUBSCRIPTION").ToList() }
//                     };

//                     foreach (var kvp in rolesToModules)
//                     {
//                         var role = kvp.Key;
//                         var roleModulesList = kvp.Value;

//                         foreach (var module in roleModulesList)
//                         {
//                             var roleModule = new RoleModule
//                             {
//                                 RoleId = role.Id,
//                                 ModuleId = module.Id,
//                                 IsActive = true,
//                                 CreatedAt = DateTime.UtcNow
//                             };
//                             await _context.RoleModules.AddAsync(roleModule);
//                             await _context.SaveChangesAsync();

//                             // Assign Permissions
//                             // Org Admin gets ALL permissions for their modules
//                             // Students/Instructors get VIEW only (logic simplified)
//                             var validPermsForModule = modulePermissions.Where(mp => mp.ModuleId == module.Id).Select(mp => mp.PermissionId).ToList();
                            
//                             foreach (var permId in validPermsForModule)
//                             {
//                                 var permFn = permissions.First(p => p.Id == permId);
//                                 bool grant = true;

//                                 if (role.Code == "STUDENT")
//                                 {
//                                     // Students only get VIEW permissions
//                                     if (!permFn.Code.Contains("_VIEW") && !permFn.Code.Contains("_DOWNLOAD")) grant = false;
//                                 }
//                                 if (role.Code == "INSTRUCTOR")
//                                 {
//                                      // Instructor gets everything EXCEPT Delete/Admin functions
//                                     if (permFn.Code.Contains("_DELETE") || permFn.Code == "ROLE_ASSIGN" || permFn.Code == "USER_PERMISSION_ASSIGN") grant = false;
//                                 }

//                                 if (grant)
//                                 {
//                                     await _context.RoleModulePermissions.AddAsync(new RoleModulePermission
//                                     {
//                                         RoleModuleId = roleModule.Id,
//                                         PermissionId = permId,
//                                         IsActive = true,
//                                         CreatedAt = DateTime.UtcNow
//                                     });
//                                 }
//                             }
//                         }
//                     }
//                     await _context.SaveChangesAsync();

//                     // 7. Ensure Users (Super Admin)
//                     if (superAdminUser == null)
//                     {
//                         superAdminUser = new User
//                         {
//                             FirstName = "Super",
//                             LastName = "Admin",
//                             Email = "admin@lms.com",
//                             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default Password
//                             CreatedAt = DateTime.UtcNow,
//                             Mobile = "0000000000",
//                             IsDeleted = false
//                         };
//                         await _context.Users.AddAsync(superAdminUser);
//                         await _context.SaveChangesAsync();
//                     }

//                     // Re-assign Super Admin Role
//                     await _context.UserRoles.AddAsync(new UserRole
//                     {
//                         UserId = superAdminUser.Id,
//                         RoleId = superAdminRole.Id,
//                         IsActive = true,
//                         CreatedAt = DateTime.UtcNow
//                     });

//                     // 8. Create Demo Data (Org, OrgAdmin)
//                     var demoOrg = new Organization
//                     {
//                         Name = "SoulCode Academy",
//                         Code = "SOUL01", 
//                         Website = "https://soulcode.com",
//                         IsActive = true,
//                         CreatedAt = DateTime.UtcNow
//                     };
//                     await _context.Organizations.AddAsync(demoOrg);
//                     await _context.SaveChangesAsync();

//                     var orgAdminUser = new User
//                     {
//                         FirstName = "Org",
//                         LastName = "Admin",
//                         Email = "orgadmin@soulcode.com",
//                         PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//                         CreatedAt = DateTime.UtcNow,
//                         TenantId = demoOrg.Id,
//                         Mobile = "9876543211",
//                         IsDeleted = false
//                     };
//                     await _context.Users.AddAsync(orgAdminUser);
//                     await _context.SaveChangesAsync();

//                     await _context.UserRoles.AddAsync(new UserRole
//                     {
//                         UserId = orgAdminUser.Id,
//                         RoleId = orgAdminRole.Id, // ORGANIZATION_ADMIN
//                         IsActive = true,
//                         CreatedAt = DateTime.UtcNow
//                     });
                    
//                     var studentUser = new User
//                     {
//                         FirstName = "Student",
//                         LastName = "User",
//                         Email = "student@soulcode.com",
//                         PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//                         CreatedAt = DateTime.UtcNow,
//                         TenantId = demoOrg.Id,
//                         Mobile = "9876543212",
//                         IsDeleted = false
//                     };
//                     await _context.Users.AddAsync(studentUser);
//                     await _context.SaveChangesAsync();

//                     await _context.UserRoles.AddAsync(new UserRole
//                     {
//                         UserId = studentUser.Id,
//                         RoleId = studentRole.Id,
//                         IsActive = true,
//                         CreatedAt = DateTime.UtcNow
//                     });

//                     await _context.SaveChangesAsync();
//                     await transaction.CommitAsync();
//                 }
//                 catch (Exception)
//                 {
//                     await transaction.RollbackAsync();
//                     throw;
//                 }
//             });
//         }

//         private async Task EnsureSpExistsAsync()
//         {
//             var baseDir = AppContext.BaseDirectory;
//             var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")); 
//             var spPath = Path.Combine(projectRoot, "sp_UpdateEntityName.sql");
//             await RunSqlScriptAsync(spPath);
//         }

//         public async Task SyncAdminPermissionsScriptsAsync()
//         {
//             try 
//             {
//                 // Ensure SP exists first
//                 await EnsureSpExistsAsync();

//                 var baseDir = AppContext.BaseDirectory;
//                 var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")); 

//                 // Execute Sync Script for Permissions/Roles (logic is complex, kept in SQL for now)
//                 var syncPath = Path.Combine(projectRoot, "Sync_OrgAdmin_Permissions.sql");
//                 await RunSqlScriptAsync(syncPath);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error executing custom SQL scripts: {ex.Message}");
//                 throw;
//             }
//         }

//         public async Task SyncModulesScriptAsync()
//         {
//              try 
//             {
//                 // 1. Ensure SP exists
//                 await EnsureSpExistsAsync();

//                 // 2. Use Reflection to get all Module Codes from C# ModuleCodes class
//                 var moduleType = typeof(ModuleCodes);
//                 var debugFields = moduleType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                
//                 foreach (var field in debugFields)
//                 {
//                     if (field.IsLiteral && !field.IsInitOnly)
//                     {
//                         var code = field.GetValue(null)?.ToString();
//                         if (!string.IsNullOrEmpty(code))
//                         {
//                             var name = ConvertCodeToName(code);
//                             // Upsert Module using the SP
//                             await _context.Database.ExecuteSqlRawAsync($"EXEC sp_UpsertEntityName 'Module', '{code}', '{name}', NULL");
//                         }
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error executing Sync Modules script: {ex.Message}");
//                 throw;
//             }
//         }

//         private string ConvertCodeToName(string code)
//         {
//             // Simple helper: "USER_MANAGEMENT" -> "User Management", "COURSE" -> "Course"
//             if (string.IsNullOrEmpty(code)) return code;
//             var parts = code.Split('_');
//             for (int i = 0; i < parts.Length; i++)
//             {
//                 parts[i] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[i].ToLower());
//             }
//             return string.Join(" ", parts);
//         }

//         private async Task RunSqlScriptAsync(string filePath)
//         {
//             if (File.Exists(filePath))
//             {
//                 var script = await File.ReadAllTextAsync(filePath);
//                 var commands = System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
//                 foreach (var command in commands)
//                 {
//                     if (!string.IsNullOrWhiteSpace(command))
//                     {
//                         try { await _context.Database.ExecuteSqlRawAsync(command); } catch { /* Ignore specific errors if needed */ }
//                     }
//                 }
//             }
//         }
//     }
// }