using API_RESTFUL_CRUD.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API_RESTFUL_CRUD.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly LescanoWebContext _context;
        public BasicAuthenticationHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock,
                LescanoWebContext context)
            : base(options, logger, encoder, clock)
        {
            _context = context;

        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("No se encontró el encabezado de autoorización.");
            }

            try
            {
                var auth = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var bythes = Convert.FromBase64String(auth.Parameter);
                string[] credentials = Encoding.UTF8.GetString(bythes).Split(":");
                string nombreusuario = credentials[0];
                string pass = credentials[1];

                Usuario usuario = _context.Usuarios.Where(u => u.NombreUsuario == nombreusuario && u.Password == pass).FirstOrDefault();

                if(usuario == null)
                    AuthenticateResult.Fail("Usuario y/o clave inválidos");
                else
                {
                    // creo un ticket de autenticación el cual voy a pasar como parámetro en el Success
                    var claims = new[] { new Claim(ClaimTypes.Name, usuario.NombreUsuario) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }
            catch (Exception)
            {
                return AuthenticateResult.Fail("Error de autenticación");
            }
            return AuthenticateResult.Fail("Error de autenticación");
            
        }
    }
}
