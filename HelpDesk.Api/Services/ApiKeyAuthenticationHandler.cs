using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HelpDesk.Api.Services
{
    public sealed class ApiKeyAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-Key", out var key))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var expectedKey = _configuration["Security:ApiKey"];

            if (string.IsNullOrWhiteSpace(expectedKey) ||
                !string.Equals(key, expectedKey, StringComparison.Ordinal))
            {
                return Task.FromResult(
                    AuthenticateResult.Fail("Ungültiger API-Schlüssel."));
            }

            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "HelpDesk-Client") },
                Scheme.Name);

            return Task.FromResult(
                AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new ClaimsPrincipal(identity),
                        Scheme.Name)));
        }
    }
}