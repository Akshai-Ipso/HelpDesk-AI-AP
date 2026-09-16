using HelpDesk.Api.DTOs;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers
{
    /// <summary>
    /// Stellt die Anmeldung und JWT-Erzeugung bereit.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Meldet einen Testbenutzer an und gibt ein JWT zurück.
        /// </summary>
        /// <param name="anfrage">Benutzername und Passwort.</param>
        /// <returns>JWT, Gültigkeitszeitpunkt und Benutzerrolle.</returns>
        /// <response code="200">Die Anmeldung war erfolgreich.</response>
        /// <response code="400">Die Eingabedaten sind ungültig.</response>
        /// <response code="401">Benutzername oder Passwort ist falsch.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(
            typeof(LoginAntwortDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        public ActionResult<LoginAntwortDto> Login(
            LoginAnfrageDto anfrage)
        {
            var ergebnis = _authService.Anmelden(anfrage);

            if (ergebnis is null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Nicht autorisiert",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Benutzername oder Passwort ist falsch.",
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(ergebnis);
        }
    }
}
