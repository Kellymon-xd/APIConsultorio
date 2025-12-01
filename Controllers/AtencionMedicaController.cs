using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtencionMedicaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AtencionMedicaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Listado de atenciones
        [HttpGet]
        public async Task<ActionResult> GetAtenciones()
        {
            var lista = await _context.AtencionMedica
                .Include(a => a.Cita)
                .ThenInclude(c => c.Paciente)
                .Include(a => a.Cita)
                .ThenInclude(c => c.Medico)
                .Select(a => new
                {
                    a.ID_Atencion,
                    a.Fecha_Atencion,
                    a.Motivo_Consulta,
                    a.Diagnostico,
                    a.Observaciones,
                    ID_Cita = a.Cita.ID_Cita,
                    NombrePaciente = a.Cita.Paciente.Nombre + " " + a.Cita.Paciente.Apellido,
                    NombreMedico = a.Cita.Medico.Horario_Atencion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: api/AtencionMedica/paciente/{idPaciente}
        [HttpGet("paciente/{idPaciente}")]
        public async Task<ActionResult> GetAtencionesPorPaciente(int idPaciente)
        {
            var atenciones = await _context.AtencionMedica
                .Where(a => a.Cita.ID_Paciente == idPaciente)
                .OrderByDescending(a => a.Fecha_Atencion)
                .Select(a => new AtencionMedicaDTO
                {
                    Id_Atencion = a.ID_Atencion,
                    Fecha = a.Fecha_Atencion,
                    Motivo = a.Motivo_Consulta,
                    Diagnostico = a.Diagnostico,
                    Tratamiento = a.Observaciones
                })
                .ToListAsync();

            if (atenciones == null || atenciones.Count == 0)
                return NoContent();

            return Ok(atenciones);
        }



        // POST: Crear atención
        [HttpPost]
        public async Task<ActionResult> CrearAtencion(CrearAtencionDTO dto)
        {
            var atencion = new AtencionMedica
            {
                ID_Cita = dto.ID_Cita,
                Motivo_Consulta = dto.Motivo_Consulta,
                Diagnostico = dto.Diagnostico,
                Observaciones = dto.Observaciones,
                Fecha_Atencion = dto.Fecha
            };

            _context.AtencionMedica.Add(atencion);
            await _context.SaveChangesAsync();

            return Ok("Atención creada correctamente.");
        }

        // PUT: Actualizar atención
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarAtencion(int id, CrearAtencionDTO dto)
        {
            var atencion = await _context.AtencionMedica.FindAsync(id);
            if (atencion == null) return NotFound();

            atencion.Motivo_Consulta = dto.Motivo_Consulta;
            atencion.Diagnostico = dto.Diagnostico;
            atencion.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();
            return Ok("Atención actualizada correctamente.");
        }

        // DELETE: Eliminar atención
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAtencion(int id)
        {
            var atencion = await _context.AtencionMedica.FindAsync(id);
            if (atencion == null) return NotFound();

            _context.AtencionMedica.Remove(atencion);
            await _context.SaveChangesAsync();
            return Ok("Atención eliminada correctamente.");
        }
    }
}
