using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
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
        private readonly LogEventos log = new LogEventos();

        public AtencionMedicaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Listado de atenciones
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> GetAtenciones()
        {
            log.setMensaje("Solicitando lista de atenciones médicas...");
            log.informacion();

            try
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

                log.setMensaje($"Total de atenciones encontradas: {lista.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, lista);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener las atenciones médicas");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

        // ============================================================
        // GET: api/AtencionMedica/paciente/{idPaciente}
        // ============================================================
        [HttpGet("paciente/{idPaciente}")]
        public async Task<ActionResult> GetAtencionesPorPaciente(int idPaciente)
        {
            log.setMensaje($"Solicitando atenciones para paciente con ID {idPaciente}");
            log.informacion();

            try
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
                {
                    log.setMensaje("El paciente no tiene atenciones registradas.");
                    log.informacion();
                    return StatusCode(StatusCodes.Status204NoContent);
                }

                log.setMensaje($"Atenciones encontradas para el paciente: {atenciones.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, atenciones);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener atenciones del paciente");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

        // ============================================================
        // POST: Crear atención
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearAtencion(CrearAtencionDTO dto)
        {
            log.setMensaje("Creando nueva atención médica...");
            log.informacion();

            try
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

                log.setMensaje("Atención médica creada correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status201Created,
                    new { mensaje = "Atención creada correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear atención médica");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

        // ============================================================
        // PUT: Actualizar atención
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarAtencion(int id, CrearAtencionDTO dto)
        {
            log.setMensaje($"Actualizando atención con ID {id}");
            log.informacion();

            try
            {
                var atencion = await _context.AtencionMedica.FindAsync(id);

                if (atencion == null)
                {
                    log.setMensaje("Atención médica no encontrada");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                atencion.Motivo_Consulta = dto.Motivo_Consulta;
                atencion.Diagnostico = dto.Diagnostico;
                atencion.Observaciones = dto.Observaciones;

                await _context.SaveChangesAsync();

                log.setMensaje("Atención médica actualizada correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK,
                    new { mensaje = "Atención actualizada correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al actualizar atención médica");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

        // ============================================================
        // DELETE: Eliminar atención
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAtencion(int id)
        {
            log.setMensaje($"Eliminando atención con ID {id}");
            log.informacion();

            try
            {
                var atencion = await _context.AtencionMedica.FindAsync(id);

                if (atencion == null)
                {
                    log.setMensaje("Atención médica no encontrada");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                _context.AtencionMedica.Remove(atencion);
                await _context.SaveChangesAsync();

                log.setMensaje("Atención médica eliminada correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK,
                    new { mensaje = "Atención eliminada correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al eliminar atención médica");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }
    }
}