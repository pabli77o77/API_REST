using API_RESTFUL_CRUD.Controllers.Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_RESTFUL_CRUD.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }

        public DbSet<Persona> Persona { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
    }
}
