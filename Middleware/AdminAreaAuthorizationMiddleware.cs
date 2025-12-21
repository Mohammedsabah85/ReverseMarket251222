using Microsoft.AspNetCore.Identity;
using ReverseMarket.Models.Identity;

namespace ReverseMarket.Middleware
{
    public class AdminAreaAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminAreaAuthorizationMiddleware> _logger;

        public AdminAreaAuthorizationMiddleware(RequestDelegate next, ILogger<AdminAreaAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            try
            {
                var path = context.Request.Path.Value?.ToLower();

                // Check if the request is for Admin area
                if (path != null && path.StartsWith("/admin"))
                {
                    // Check if user is authenticated
                    if (!context.User.Identity?.IsAuthenticated ?? true)
                    {
                        _logger.LogWarning("Unauthorized access attempt to Admin area from IP: {IP}",
                            context.Connection.RemoteIpAddress);
                        context.Response.Redirect("/Account/Login?returnUrl=" +
                            Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                        return;
                    }

                    // Check if user has Admin role
                    if (!context.User.IsInRole("Admin"))
                    {
                        _logger.LogWarning("Access denied to Admin area for user: {User} from IP: {IP}",
                            context.User.Identity?.Name ?? "Unknown", context.Connection.RemoteIpAddress);

                        // Redirect to access denied page
                        context.Response.Redirect("/Error/AccessDenied");
                        return;
                    }

                    _logger.LogInformation("Admin area access granted for user: {User}",
                        context.User.Identity?.Name ?? "Unknown");
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdminAreaAuthorizationMiddleware");

                // ✅ إعادة التوجيه لصفحة خطأ بدلاً من رمي الاستثناء
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    // Extension method to register the middleware
    public static class AdminAreaAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminAreaAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminAreaAuthorizationMiddleware>();
        }
    }
}