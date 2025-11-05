using Microsoft.AspNetCore.Localization;

namespace DentalCareManagmentSystem.Web.Middleware;

public class RouteDataRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var culture = httpContext.GetRouteValue("culture")?.ToString();

        if (string.IsNullOrWhiteSpace(culture))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var providerResultCulture = new ProviderCultureResult(culture);
        return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
    }
}
