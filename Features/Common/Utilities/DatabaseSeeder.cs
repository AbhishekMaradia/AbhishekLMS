using LMS_SoulCode.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using LMS_SoulCode.Features.Common;

namespace LMS_SoulCode.Features.Common.Utilities
{
    public class DatabaseSeeder
    {
        private readonly LmsDbContext _context;

        public DatabaseSeeder(LmsDbContext context)
        {
            _context = context;
        }

        // Standardized Sync using Stored Procedures
        public async Task SyncSecurityNodesAsync()
        {
            try 
            {
                // 1. Sync Modules using Stored Procedure
                var moduleType = typeof(ModuleCodes);
                var modFields = moduleType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                
                foreach (var field in modFields)
                {
                    if (field.IsLiteral && !field.IsInitOnly)
                    {
                        var code = field.GetValue(null)?.ToString();
                        if (!string.IsNullOrEmpty(code))
                        {
                            var name = ConvertCodeToName(code);
                            // EXEC sp_UpsertEntityName '{EntityType}', '{Code}', '{Name}', {IsActive}
                            await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpsertEntityName {0}, {1}, {2}, {3}", "Module", code, name, 1);
                        }
                    }
                }

                // 2. Sync Permissions using Stored Procedure
                var permType = typeof(PermissionCodes);
                var permFields = permType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                
                foreach (var field in permFields)
                {
                    if (field.IsLiteral && !field.IsInitOnly)
                    {
                        var code = field.GetValue(null)?.ToString();
                        if (!string.IsNullOrEmpty(code))
                        {
                            var name = ConvertCodeToName(code);
                            await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpsertEntityName {0}, {1}, {2}, {3}", "Permission", code, name, 1);
                        }
                    }
                }

                var permissions = await _context.Permissions.ToListAsync();
                var modules = await _context.Modules.ToListAsync();

                foreach (var perm in permissions)
                {
                    string targetModuleCode = "";

                    if (perm.Code.StartsWith("ROLE_MODULE_PERMISSION_")) targetModuleCode = ModuleCodes.ROLE_MODULE;
                    else if (perm.Code.StartsWith("ROLE_MODULE_")) targetModuleCode = ModuleCodes.ROLE_MODULE;
                    else if (perm.Code.StartsWith("USER_ROLE_")) targetModuleCode = ModuleCodes.USER_ROLE;
                    else if (perm.Code.StartsWith("MODULE_PERMISSION_")) targetModuleCode = ModuleCodes.MODULE;
                    else if (perm.Code.StartsWith("ROLE_")) targetModuleCode = ModuleCodes.ROLE;
                    else if (perm.Code.StartsWith("MODULE_")) targetModuleCode = ModuleCodes.MODULE;
                    else if (perm.Code.StartsWith("PERMISSION_")) targetModuleCode = ModuleCodes.PERMISSION;
                    else if (perm.Code.StartsWith("COURSE_")) targetModuleCode = ModuleCodes.COURSE;
                    else if (perm.Code.StartsWith("CATEGORY_")) targetModuleCode = ModuleCodes.CATEGORY;
                    else if (perm.Code.StartsWith("USER_")) targetModuleCode = ModuleCodes.USER;
                    else if (perm.Code.StartsWith("VIDEO_")) targetModuleCode = ModuleCodes.VIDEO;
                    else if (perm.Code.StartsWith("ORGANIZATION_")) targetModuleCode = ModuleCodes.ORGANIZATION;
                    else if (perm.Code.StartsWith("GROUP_")) targetModuleCode = ModuleCodes.GROUP;
                    else if (perm.Code.StartsWith("CERTIFICATE_")) targetModuleCode = ModuleCodes.CERTIFICATE;
                    else if (perm.Code.StartsWith("SUBSCRIPTION_")) targetModuleCode = ModuleCodes.SUBSCRIPTION;
                    else if (perm.Code.StartsWith("REPORT_")) targetModuleCode = ModuleCodes.REPORT;
                    else if (perm.Code.StartsWith("FILE_")) targetModuleCode = ModuleCodes.FILE_MANAGEMENT;
                    else if (perm.Code.StartsWith("ATTENDANCE_")) targetModuleCode = ModuleCodes.ATTENDANCE;

                    if (!string.IsNullOrEmpty(targetModuleCode))
                    {
                        var mod = modules.FirstOrDefault(m => m.Code == targetModuleCode);
                        if (mod != null)
                        {
                            var exists = await _context.ModulePermissions.AnyAsync(mp => mp.ModuleId == mod.Id && mp.PermissionId == perm.Id);
                            if (!exists)
                            {
                                _context.ModulePermissions.Add(new LMS_SoulCode.Features.UserPermissions.Models.ModulePermission
                                {
                                    ModuleId = mod.Id,
                                    PermissionId = perm.Id,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("[LMS SYNC] Security Nodes Synchronized via SP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LMS SYNC ERROR] {ex.Message}");
                throw;
            }
        }

        private string ConvertCodeToName(string code)
        {
            if (string.IsNullOrEmpty(code)) return code;
            var parts = code.Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[i].ToLower());
            }
            return string.Join(" ", parts);
        }
    }
}