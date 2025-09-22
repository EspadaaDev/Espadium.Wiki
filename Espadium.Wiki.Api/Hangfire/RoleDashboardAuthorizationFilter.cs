using Hangfire.Dashboard;

namespace Espadium.Wiki.Api.HangfireAuth
{
    internal class RoleDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _role;
        public RoleDashboardAuthorizationFilter(string role) { _role = role; }
        public bool Authorize(DashboardContext context)
        {
            var http = context.GetHttpContext();
            return http.User?.IsInRole(_role) == true;
        }
    }
}

