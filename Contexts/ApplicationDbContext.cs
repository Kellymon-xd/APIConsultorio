using ApiConsultorio.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<ActividadUsuario> ActividadUsuarios { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<TipoContrato> TipoContrato { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<EstadoCita> EstadoCita { get; set; }
        public DbSet<AtencionMedica> AtencionMedica { get; set; }
        public DbSet<AntecedentesMedico> AntecedentesMedicos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // RELACIÓN 1:1 Usuario ↔ ActividadUsuario
            // (ActividadUsuario depende de Usuario)
            // ============================================================
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.ActividadUsuario)
                .WithOne(a => a.Usuario)
                .HasForeignKey<ActividadUsuario>(a => a.Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================================
            // RELACIÓN 1:1 Usuario ↔ Medico
            // (Medico depende de Usuario)
            // ============================================================
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Medico)
                .WithOne(m => m.Usuario)
                .HasForeignKey<Medico>(m => m.Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
