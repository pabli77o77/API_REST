using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_RESTFUL_CRUD.Models
{
    public class RefreshRequest
    {
        /// <summary>
        /// Envía el token actual
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Recibe el token actualizado (cuando se actualice el token)
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
