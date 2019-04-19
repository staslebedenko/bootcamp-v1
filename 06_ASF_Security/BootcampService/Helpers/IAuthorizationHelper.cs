using System.Security.Claims;

namespace Microsoft.Examples.Helpers
{
    interface IAuthorizationHelper
    {
        bool ValidateClaims(ClaimsPrincipal authorizedUser, string fancyClaimValue);
    }
}
