using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Listado de citas con nombres y especialidad
        [HttpGet]
        public async Task<ActionResult> GetCitas()
        {
            var lista = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .Include(c => c.EstadoCita)
                .ThenInclude(e => e.Descripcion)
                .Select(c => new
                {
                    c.ID_Cita,
                    NombrePaciente = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                    NombreMedico = c.Medico.Horario_Atencion != null ? c.Medico.Horario_Atencion : "",
                    Especialidad = c.Medico.ID_Especialidad,
                    c.Fecha_Cita,
                    c.Hora_Cita,
                    Estado = c.EstadoCita.Descripcion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: Detalle de una cita por ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .Include(c => c.EstadoCita)
                .FirstOrDefaultAsync(c => c.ID_Cita == id);

            if (cita == null) return NotFound();

            return Ok(cita);
        }

        // POST: Crear cita
        [HttpPost]
        public async Task<ActionResult> CrearCita(CrearCitaDTO dto)
        {
            var cita = new Cita
            {
                ID_Paciente = dto.ID_Paciente,
                ID_Medico = dto.ID_Medico,
                Fecha_Cita = dto.Fecha_Cita,
                Hora_Cita = dto.Hora_Cita,
                ID_Estado_Cita = dto.ID_Estado_Cita
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return Ok("Cita creada correctamente.");
        }

        // PUT: Actualizar cita
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarCita(int id, CrearCitaDTO dto)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.ID_Paciente = dto.ID_Paciente;
            cita.ID_Medico = dto.ID_Medico;
            cita.Fecha_Cita = dto.Fecha_Cita;
            cita.Hora_Cita = dto.Hora_Cita;
            cita.ID_Estado_Cita = dto.ID_Estado_Cita;

            await _context.SaveChangesAsync();
            return Ok("Cita actualizada correctamente.");
        }

        // DELETE: Eliminar cita
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
            return Ok("Cita eliminada correctamente.");
        }
    }
}
