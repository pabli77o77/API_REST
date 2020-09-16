using System;
using System.Collections.Generic;

namespace API_RESTFUL_CRUD.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            ActualizarTokens = new HashSet<ActualizarToken>();
        }

        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public int? IdPersona { get; set; }
        public int IdRole { get; set; }

        public virtual Persona IdPersonaNavigation { get; set; }
        public virtual ICollection<ActualizarToken> ActualizarTokens { get; set; }
    }
}
