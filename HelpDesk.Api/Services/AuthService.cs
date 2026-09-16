using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HelpDesk.Api.Services
{
    /// <summary>
    /// Prüft Testbenutzer und erzeugt JWTs.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc />
        public LoginAntwortDto? Anmelden(LoginAnfrageDto anfrage)
        {
            var testBenutzer = _configuration
                .GetSection("TestBenutzer")
                .Get<List<TestBenutzer>>()
                ?? new List<TestBenutzer>();

            var benutzer = testBenutzer.FirstOrDefault(b =>
                b.Benutzername == anfrage.Benutzername &&
                b.Passwort == anfrage.Passwort);

            if (benutzer is null)
            {
                _logger.LogWarning(
                    "Fehlgeschlagener Loginversuch für Benutzer {Benutzername}.",
                    anfrage.Benutzername);

                return null;
            }

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Die JWT-Konfiguration 'Jwt:Issuer' fehlt.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Die JWT-Konfiguration 'Jwt:Audience' fehlt.");

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Die JWT-Konfiguration 'Jwt:Key' fehlt.");

            var gueltigkeitsdauer =
                _configuration.GetValue<int>(
                    "Jwt:GueltigkeitsdauerMinuten");

            if (gueltigkeitsdauer <= 0)
            {
                gueltigkeitsdauer = 60;
            }

            var gueltigBis =
                DateTime.UtcNow.AddMinutes(gueltigkeitsdauer);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    benutzer.Benutzername),

                new Claim(
                    ClaimTypes.Role,
                    benutzer.Rolle),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var sicherheitsSchluessel =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var signatur =
                new SigningCredentials(
                    sicherheitsSchluessel,
                    SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: gueltigBis,
                signingCredentials: signatur);

            var tokenText =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            _logger.LogInformation(
                "Benutzer {Benutzername} wurde mit Rolle {Rolle} angemeldet.",
                benutzer.Benutzername,
                benutzer.Rolle);

            return new LoginAntwortDto
            {
                Token = tokenText,
                GueltigBis = gueltigBis,
                Rolle = benutzer.Rolle
            };
        }
    }
}
