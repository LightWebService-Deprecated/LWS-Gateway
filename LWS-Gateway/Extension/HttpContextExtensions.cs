using System.Collections.Generic;
using LWS_Gateway.Model;
using Microsoft.AspNetCore.Http;

namespace LWS_Gateway.Extension;

public static class HttpContextExtensions
{
    private const string UserEmail = "userEmail";
    private const string UserRole = "userRole";
    
    // User Email Related
    public static void SetUserEmail(this HttpContext context, string userEmail)
    {
        context.Items[UserEmail] = userEmail;
    }
    public static string GetUserEmail(this HttpContext context) => context.Items[UserEmail] as string;
    public static bool IsUserEmailExists(this HttpContext context) => context.Items.ContainsKey(UserEmail);
    
    // User Role Related
    public static void SetUserRole(this HttpContext context, HashSet<AccountRole> accountRoles)
    {
        context.Items[UserRole] = accountRoles;
    }
    public static HashSet<AccountRole> GetUserRole(this HttpContext context) => (HashSet<AccountRole>)context.Items[UserRole]!;
    public static bool IsUserRoleExists(this HttpContext context) => context.Items.ContainsKey(UserRole);
}