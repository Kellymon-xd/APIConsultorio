using ApiConsultorio.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<ActividadUsuario> ActividadUsuarios { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<TipoContrato> TipoContrato { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<EstadoCita> EstadoCita { get; set; }
        public DbSet<AtencionMedica> AtencionMedicas { get; set; }
        public DbSet<AntecedenteMedico> AntecedentesMedicos { get; set; }
    }
}
