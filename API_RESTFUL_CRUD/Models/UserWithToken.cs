using API_RESTFUL_CRUD.Controllers.Entidades;
using API_RESTFUL_CRUD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_RESTFUL_CRUD.Models
{
    public class UserWithToken : Usuario
    {
        /// <summary>
        /// Envía el token actual
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Recibe el token actualizado (cuando se actualice el token)
        /// </summary>
        public string RefreshToken { get; set; }

        public UserWithToken(Usuario user)
        {
            this.IdUsuario = user.IdUsuario;
            this.NombreUsuario = user.NombreUsuario;
            this.IdPersona = user.IdPersona;
            
            this.IdRole = user.IdRole;
            
        }
    }
}
