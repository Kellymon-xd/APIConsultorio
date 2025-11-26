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
        private readonly LogEventos _log = new LogEventos();

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // GET: Listado de citas
        // ============================================
        [HttpGet]
        public async Task<ActionResult> GetCitas()
        {
            try
            {
                _log.setMensaje("Intentando obtener listado de citas...");
                _log.informacion();

                var lista = await _context.Citas
                    .Include(c => c.Paciente)
                    .Include(c => c.Medico)
                        .ThenInclude(m => m.Usuario)
                    .Include(c => c.Medico)
                        .ThenInclude(m => m.Especialidad)
                    .Include(c => c.EstadoCita)
                    .Select(c => new CitaListadoDTO
                    {
                        ID_Cita = c.ID_Cita,
                        NombrePaciente = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                        NombreMedico = c.Medico.Usuario.Nombre + " " + c.Medico.Usuario.Apellido,
                        Especialidad = c.Medico.Especialidad.Nombre_Especialidad,
                        Fecha_Cita = c.Fecha_Cita,
                        Hora_Cita = c.Hora_Cita,
                        Estado = c.EstadoCita.Descripcion
                    })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                _log.informacion(ex);
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        // ============================================
        // POST: Crear cita
        // ============================================
        [HttpPost]
        public async Task<ActionResult> CrearCita(CrearCitaDTO dto)
        {
            try
            {
                _log.setMensaje("Intentando crear una cita...");
                _log.informacion();

                var cita = new Cita
                {
                    ID_Paciente = dto.ID_Paciente,
                    ID_Medico = dto.ID_Medico,
                    Fecha_Cita = dto.Fecha_Cita,
                    Hora_Cita = dto.Hora_Cita,
                    ID_Estado_Cita = dto.ID_Estado_Cita
                };

                await _context.Citas.AddAsync(cita);
                await _context.SaveChangesAsync();

                return StatusCode(201, "Cita creada correctamente.");
            }
            catch (Exception ex)
            {
                _log.informacion(ex);
                return StatusCode(500, "Error al crear la cita.");
            }
        }

        // ============================================
        // PATCH: Cambiar estado de cita
        // ============================================
        [HttpPatch("{id}/estado")]
        public async Task<ActionResult> CambiarEstado(int id, CambiarEstadoCitaDTO dto)
        {
            try
            {
                _log.setMensaje($"Intentando cambiar estado de la cita {id}...");
                _log.informacion();

                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    _log.setMensaje($"Cita {id} no encontrada.");
                    _log.informacion();
                    return NotFound("La cita no existe.");
                }

                cita.ID_Estado_Cita = dto.ID_Estado_Cita;

                await _context.SaveChangesAsync();

                return Ok("Estado de la cita actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _log.informacion(ex);
                return StatusCode(500, "Error al cambiar el estado de la cita.");
            }
        }
    }
}
