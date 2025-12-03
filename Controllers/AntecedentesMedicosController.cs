using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntecedentesMedicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

        public AntecedentesMedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Listado de antecedentes
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> GetAntecedentes()
        {
            log.setMensaje("Solicitando lista de antecedentes médicos...");
            log.informacion();

            try
            {
                var lista = await _context.AntecedentesMedicos
                    .Include(a => a.Paciente)
                    .Select(a => new
                    {
                        a.ID_Antecedente,
                        a.ID_Paciente,
                        NombrePaciente = a.Paciente.Nombre + " " + a.Paciente.Apellido,
                        a.Alergias,
                        a.Enfermedades_Cronicas,
                        a.Observaciones_Generales,
                        a.Fecha_Registro
                    })
                    .ToListAsync();

                log.setMensaje($"Total de antecedentes encontrados: {lista.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, lista);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener los antecedentes médicos");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al obtener los antecedentes.");
            }
        }

        // ============================================================
        // GET: Antecedente por ID del Paciente
        // ============================================================
        [HttpGet("paciente/{idPaciente}")]
        public async Task<ActionResult> GetAntecedentePorPaciente(int idPaciente)
        {
            log.setMensaje($"Solicitando antecedentes para el paciente {idPaciente}");
            log.informacion();

            try
            {
                var antecedente = await _context.AntecedentesMedicos
                    .Where(a => a.ID_Paciente == idPaciente)
                    .Select(a => new AntecedentesDetalleDTO
                    {
                        ID_Antecedente = a.ID_Antecedente,
                        ID_Paciente = a.ID_Paciente,
                        Alergias = a.Alergias,
                        Enfermedades_Cronicas = a.Enfermedades_Cronicas,
                        Observaciones_Generales = a.Observaciones_Generales,
                        Fecha_Registro = a.Fecha_Registro.ToString("dd/MM/yyyy HH:mm")
                    })
                    .FirstOrDefaultAsync();

                if (antecedente == null)
                {
                    log.setMensaje("El paciente no tiene antecedentes registrados.");
                    log.informacion();
                    return StatusCode(StatusCodes.Status204NoContent);
                }

                log.setMensaje("Antecedentes encontrados para el paciente.");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, antecedente);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener antecedentes por paciente");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al obtener los antecedentes del paciente.");
            }
        }

        // ============================================================
        // POST: Crear antecedentes
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearAntecedente(CrearAntecedentesDTO dto)
        {
            log.setMensaje("Creando nuevo antecedente médico...");
            log.informacion();

            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);

            try
            {
                var antecedente = new AntecedentesMedico
                {
                    ID_Paciente = dto.ID_Paciente,
                    Alergias = dto.Alergias,
                    Enfermedades_Cronicas = dto.Enfermedades_Cronicas,
                    Observaciones_Generales = dto.Observaciones_Generales,
                    Fecha_Registro = DateTime.Now
                };

                _context.AntecedentesMedicos.Add(antecedente);
                await _context.SaveChangesAsync();

                log.setMensaje("Antecedentes creados correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status201Created,
                    new { mensaje = "Antecedentes creados correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear antecedentes médicos");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al crear los antecedentes.");
            }
        }

        // ============================================================
        // PUT: Actualizar antecedentes
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarAntecedente(int id, CrearAntecedentesDTO dto)
        {
            log.setMensaje($"Actualizando antecedente con ID {id}");
            log.informacion();

            try
            {
                var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
                if (antecedente == null)
                {
                    log.setMensaje("Antecedente no encontrado");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound, "Antecedente no encontrado.");
                }

                antecedente.Alergias = dto.Alergias;
                antecedente.Enfermedades_Cronicas = dto.Enfermedades_Cronicas;
                antecedente.Observaciones_Generales = dto.Observaciones_Generales;

                await _context.SaveChangesAsync();

                log.setMensaje("Antecedente actualizado correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK,
                    new { mensaje = "Antecedentes actualizados correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al actualizar antecedentes médicos");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al actualizar los antecedentes.");
            }
        }

        // ============================================================
        // DELETE: Eliminar antecedentes
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAntecedente(int id)
        {
            log.setMensaje($"Eliminando antecedente con ID {id}");
            log.informacion();

            try
            {
                var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
                if (antecedente == null)
                {
                    log.setMensaje("Antecedente no encontrado");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound, "Antecedente no encontrado.");
                }

                _context.AntecedentesMedicos.Remove(antecedente);
                await _context.SaveChangesAsync();

                log.setMensaje("Antecedente eliminado correctamente.");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK,
                    new { mensaje = "Antecedentes eliminados correctamente." });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al eliminar antecedente médico");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al eliminar los antecedentes.");
            }
        }
    }
}