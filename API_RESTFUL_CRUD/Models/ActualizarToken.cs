using System;
using System.Collections.Generic;

namespace API_RESTFUL_CRUD.Models
{
    public partial class ActualizarToken
    {
        public int TokenId { get; set; }
        public int IdUsuario { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; }
    }
}
