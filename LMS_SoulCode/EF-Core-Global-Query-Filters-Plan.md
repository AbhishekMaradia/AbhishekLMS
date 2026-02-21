# EF Core Global Query Filters - Parameter Cleanup Plan

## ✅ **Current Status**
- Global Query Filters implemented with lambda expressions in `LmsDbContext.cs`
- Uses existing `ClaimsExtensions.GetTenantId()`
- Automatic tenant isolation working at database level

## ⚠️ **Lambda Expression Disadvantages**

### **1. Compile-Time Evaluation Issue**
```csharp
// PROBLEM: CurrentTenantId is evaluated once at DbContext creation
public int? CurrentTenantId => _httpContextAccessor?.HttpContext?.User.GetTenantId();

modelBuilder.Entity<User>().HasQueryFilter(e => 
    (CurrentTenantId == null || e.TenantId == CurrentTenantId) && !e.IsDeleted);
```
**Issue**: The tenant ID is "baked in" when the DbContext is created, not evaluated per query.

### **2. DbContext Scoping Problems**
- **Singleton DbContext**: If DbContext is accidentally registered as singleton, all users see same tenant
- **Long-lived DbContext**: Tenant ID won't change even if user context changes
- **Background Services**: No HTTP context available, CurrentTenantId will be null

### **3. Testing Difficulties**
```csharp
// Hard to test different tenant scenarios
// Can't easily mock or override tenant ID for specific tests
```

### **4. Admin/Cross-Tenant Queries**
```csharp
// Difficult to temporarily bypass filters for admin users
// Need to use IgnoreQueryFilters() which bypasses ALL filters (including soft delete)
var allTenantData = await _context.Users.IgnoreQueryFilters().ToListAsync();
```

### **5. Performance Concerns**
- **Query Plan Caching**: Different tenant IDs create different query plans
- **Memory Usage**: More query plans cached in memory
- **SQL Parameter Issues**: Tenant ID becomes part of compiled query, not a parameter

### **6. Debugging Complexity**
- **Hidden Filtering**: Developers might forget filters are applied
- **Unexpected Empty Results**: Queries return no data without obvious reason
- **SQL Tracing**: Generated SQL includes tenant filter, making debugging harder

## 🔄 **Parameter Cleanup Tasks**

### **1. UserCourseService Interface & Implementation**
Remove `int? tenantId` parameter from all methods:
- `SubscribeAsync()`
- `UnsubscribeAsync()`
- `IsSubscribedAsync()`
- `GetUserCoursesAsync()`
- `GetAllSubscribedAsync()`

### **2. UserCourseController**
Remove `CurrentTenantId` from all service calls:
- Subscribe endpoint
- Unsubscribe endpoint
- My-courses endpoint
- Check endpoint
- List endpoint
- Subscribed-List endpoint

### **3. UserCourseRepository Interface & Implementation**
Remove `int? tenantId` parameter from all methods:
- `GetAsync()`
- `IsSubscribedAsync()`
- `GetByUserAsync()`
- `GetAllSubscribedAsync()`
- `GetUserCoursesAsync()`

### **4. Other Services/Controllers (Future)**
Apply same pattern to all other features:
- CourseService & CourseController
- CategoryService & CategoryController
- CertificateService & CertificateController
- UserService & UserController
- All other tenant-aware services

## 🎯 **Key Rules**

### **REMOVE:**
- `int? tenantId` parameters from all service methods
- `CurrentTenantId` from all controller calls
- Manual tenant filtering in repositories

### **KEEP:**
- `int userId` parameters (business logic)
- `CurrentUserId` in controllers (business logic)
- User-specific filtering in repositories

## 📋 **Implementation Order**
1. Start with UserCourse feature (current focus)
2. Test thoroughly to ensure tenant isolation works
3. Apply to other features systematically
4. Final testing across all features

## 🔧 **Alternative Solutions (If Issues Arise)**

### **Option 1: Dynamic Query Filters**
```csharp
// Evaluate tenant ID per query, not per DbContext creation
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>().HasQueryFilter(e => 
        EF.Property<int?>(e, "TenantId") == GetCurrentTenantId() && !e.IsDeleted);
}

private int? GetCurrentTenantId()
{
    return _httpContextAccessor?.HttpContext?.User.GetTenantId();
}
```

### **Option 2: Repository Pattern with Manual Filtering**
```csharp
// Keep manual filtering but centralize it in base repository
public abstract class BaseTenantRepository<T> where T : BaseTenantEntity
{
    protected IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
    {
        var tenantId = _httpContextAccessor?.HttpContext?.User.GetTenantId();
        return query.Where(e => e.TenantId == tenantId && !e.IsDeleted);
    }
}
```

### **Option 3: Interceptor Pattern**
```csharp
// Use EF Core interceptors to modify queries at runtime
public class TenantQueryInterceptor : IQueryInterceptor
{
    // Modify SQL queries to add tenant filtering
}
```