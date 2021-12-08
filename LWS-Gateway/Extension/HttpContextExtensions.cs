using System.Collections.Generic;
using LWS_Gateway.Model;
using Microsoft.AspNetCore.Http;

namespace LWS_Gateway.Extension;

public static class HttpContextExtensions
{
    private const string UserId = "userId";
    private const string UserRole = "userRole";
    
    // User Email Related
    public static void SetUserId(this HttpContext context, string userId)
    {
        context.Items[UserId] = userId;
    }
    public static string GetUserId(this HttpContext context) => context.Items[UserId] as string;
    public static bool IsUserIdExists(this HttpContext context) => context.Items.ContainsKey(UserId);
    
    // User Role Related
    public static void SetUserRole(this HttpContext context, HashSet<AccountRole> accountRoles)
    {
        context.Items[UserRole] = accountRoles;
    }
    public static HashSet<AccountRole> GetUserRole(this HttpContext context) => (HashSet<AccountRole>)context.Items[UserRole]!;
    public static bool IsUserRoleExists(this HttpContext context) => context.Items.ContainsKey(UserRole);
}