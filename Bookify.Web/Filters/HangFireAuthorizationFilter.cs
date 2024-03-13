using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Bookify.Web.Filters
{
	public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		private string _policyName;

		public HangFireAuthorizationFilter(string policyName)
		{
			_policyName = policyName;
		}

		public bool Authorize([NotNull] DashboardContext context)
		{
			var httpContext = context.GetHttpContext();
			var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

			var isAuthorized = authService.AuthorizeAsync(httpContext.User, _policyName)
				.ConfigureAwait(false)
				.GetAwaiter().GetResult().Succeeded;

			return isAuthorized;
			
		}
	}
}
