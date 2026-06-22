using System.Reflection;
using LMS_SoulCode.Features.Common.Services;

namespace LMS_SoulCode.RepositoryMapping
{
    public static class RepoServiceMapping
    {
        public static void AddRepoServiceMapping(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 1. Get all classes that are not abstract and end with 'Service' or 'Repository'
            var typesToRegister = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                           (t.Name.EndsWith("Service") || t.Name.EndsWith("Repository")));

            foreach (var type in typesToRegister)
            {
                // Find the interface that follows the I[Name] convention (e.g., IUserRepository for UserRepository)
                var interfaceName = $"I{type.Name}";
                var primaryInterface = type.GetInterfaces().FirstOrDefault(i => i.Name == interfaceName);

                if (primaryInterface != null)
                {
                    services.AddScoped(primaryInterface, type);
                }
                else
                {
                    // If no matching interface is found, register the class itself
                    services.AddScoped(type);
                }
            }

            // 2. Manual Overrides for special cases (where names don't match the convention)
            // Example: ICacheService implemented by MemoryCacheService
            var cacheService = typesToRegister.FirstOrDefault(t => t.Name == "MemoryCacheService");
            if (cacheService != null)
            {
                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            // Note: EmailService might already be registered via auto-DI if it follows conventions,
            // but we can be explicit for clarity if needed.
        }
    }
}
