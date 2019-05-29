using System.Linq;
using System.Security.Claims;

namespace ChatApp.Server
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetNameIdentifierClaim(this ClaimsPrincipal self) => GetClaimValue(self, ClaimTypes.NameIdentifier);

        public static string GetClaimValue(this ClaimsPrincipal self, string claimType) => !self.HasClaim(c => c.Type == claimType) ? null : self.Claims.First(c => c.Type == claimType).Value;
    }
}