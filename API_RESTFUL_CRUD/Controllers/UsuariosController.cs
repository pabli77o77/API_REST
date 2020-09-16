using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_RESTFUL_CRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace API_RESTFUL_CRUD.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly LescanoWebContext _context;
        private readonly JWTSettings _jwtSettings;

        public UsuariosController(LescanoWebContext context, IOptions<JWTSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario()
        {
            // Leo el usuario, no necesito la contraseña porque ya estoy validado ***????***
            string nombreusuario = HttpContext.User.Identity.Name;
            var usuario = await _context.Usuarios
                                        .Where(usuario => usuario.NombreUsuario == nombreusuario)
                                        .FirstOrDefaultAsync();
            //Me aseguro que la contraseña sea nula para que no se esté pasando información confidencial en el request
            usuario.Password = null;
            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

       
        [HttpGet("Login")]
        public async Task<ActionResult<UserWithToken>> Login([FromBody] Usuario usuario)
        {
            
            usuario = await _context.Usuarios
                                .Where(u => u.NombreUsuario == usuario.NombreUsuario
                                    && u.Password == usuario.Password)
                                .FirstOrDefaultAsync();
           
            
            UserWithToken userWithToken = null;
            if (usuario != null)
            {
                ActualizarToken actualizartoken = GenerarNuevoToken();
                usuario.ActualizarTokens.Add(actualizartoken);
                await _context.SaveChangesAsync();

                userWithToken = new UserWithToken(usuario);
                userWithToken.RefreshToken = actualizartoken.Token;
            }

            if (userWithToken == null)
            {
                return NotFound();
            }

            //Registro del token
            userWithToken.AccessToken = GenerarAccessToken(usuario.IdUsuario);
            return userWithToken;
        }

        /// <summary>
        /// Toma el token caducado, mira si el usuario es válido y devuelve un usuario con el token actualizado
        /// </summary>
        /// <param name="refreshRequest"></param>
        /// <returns></returns>
        [HttpGet("ActualizarToken")]
        public async Task<ActionResult<UserWithToken>> ActualizarToken([FromBody] RefreshRequest refreshRequest) 
        {
            //Obtengo de la base de datos el usuario con el token caducado (usuario caducado)
            Usuario usuario = GetUserFromAccessToken(refreshRequest.AccessToken);
            // Verifico el token de acceso en la base
            if (usuario != null && ValidateRefreshToken(usuario, refreshRequest.RefreshToken))
            {
                //Devuelvo un usuario con el token actualizado
                UserWithToken userWithToken = new UserWithToken(usuario);
                // Genero un nuevo token para el usuario
                userWithToken.AccessToken = GenerarAccessToken(usuario.IdUsuario);

                return userWithToken;
            }
            return null;
        }

        /// <summary>
        /// Valida el token de actualización
        /// </summary>
        /// <param name="user"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private bool ValidateRefreshToken(Usuario user, string refreshToken)
        {
            // Compruebo si el token de actualizacion coincide con el id de usuario del token
            // de actualizacion que ha sido firmado por el cliente
            ActualizarToken refreshTokenUser = _context.ActualizarTokens.Where(rt => rt.Token == refreshToken)
                                                                        .OrderByDescending(rt => rt.FechaExpiracion)
                                                                        .FirstOrDefault();

            // Verifico si el token no es nulo, si pertenece al mismo usuario que recibo por parámetro y si la 
            // fecha de expiración del token es mayor a hoy
            if (refreshTokenUser != null && refreshTokenUser.IdUsuario == user.IdUsuario
                && refreshTokenUser.FechaExpiracion > DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Recibe un token caducado, verifica si el token fue firmado por esta misma API, si es del mismo usuario
        /// y si usa el mismo algoritmo de encriptación.
        /// </summary>
        /// <param name="accessToken">Token caducado que viene del cliente</param>
        /// <returns></returns>
        private Usuario GetUserFromAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey); // la que configuré en appsettings

            var tokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

            //Verifico si el token de seguridad que caducó ha sido firmado por la misma API
            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, 
                                                                        StringComparison.InvariantCultureIgnoreCase))
            {
                // Si lo encuentro:
                // Obtengo el id de usuario que firmó el token
                var userId = principle.FindFirst(ClaimTypes.Name)?.Value;
                // Verifico el id de usuario con el que tengo en la base de datos
                return _context.Usuarios.Where(u => u.IdUsuario == Convert.ToInt32(User)).FirstOrDefault();
            
            } // Sino retorno null
            return null;
        }

        /// <summary>
        /// Genera el token de aceso para el usuario
        /// </summary>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        private string GenerarAccessToken(int idUsuario) 
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey); // la que configuré en appsettings
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(idUsuario))
                }),
                Expires = DateTime.UtcNow.AddMonths(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Actualiza el token de usuario
        /// </summary>
        /// <returns></returns>
        private ActualizarToken GenerarNuevoToken() 
        {
            ActualizarToken actualizarToken = new ActualizarToken();

            var rndNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) 
            {
                rng.GetBytes(rndNumber);
                actualizarToken.Token = Convert.ToBase64String(rndNumber);
            }
            actualizarToken.FechaExpiracion = DateTime.UtcNow.AddMonths(6);

            return actualizarToken;
        }


        // PUT: api/Usuarios/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Usuarios
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.IdUsuario }, usuario);
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Usuario>> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
