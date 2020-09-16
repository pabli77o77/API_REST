using System;
using System.Collections.Generic;

namespace API_RESTFUL_CRUD.Models
{
    public partial class Persona
    {
        public Persona()
        {
            Usuarios = new HashSet<Usuario>();
        }

        public int IdPersona { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Linkedin { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
