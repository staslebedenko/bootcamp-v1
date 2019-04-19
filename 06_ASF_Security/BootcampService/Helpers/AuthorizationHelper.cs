using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Examples.Helpers
{
    public class AuthorizationHelper : IAuthorizationHelper
    {
        private readonly TelemetryClient telemetry = new TelemetryClient(TelemetryConfiguration.Active);


        public bool ValidateClaims(ClaimsPrincipal authorizedUser, string fancyClaimValue)
        {
            var userClaims = authorizedUser.Claims.ToList();
            var userEmail = userClaims.FirstOrDefault(x => x.Type.Contains("email"))?.Value;

            foreach (var claim in userClaims)
            {
                if (claim.Value != fancyClaimValue)
                {
                    continue;
                }

                this.telemetry.TrackEvent($"Authorization check success for user {userEmail}, to fancy claimValue {fancyClaimValue}, on {DateTime.Now}");
                return true;
            }

            this.telemetry.TrackEvent($"Authorization violation by user {userEmail}, to fancy claimValue {fancyClaimValue}, on {DateTime.Now}");

            return false;
        }
    }
}
