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

        public AntecedentesMedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Listado de antecedentes
        [HttpGet]
        public async Task<ActionResult> GetAntecedentes()
        {
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

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al obtener los antecedentes.");
            }
        }

        // GET: Antecedente por ID del Paciente
        [HttpGet("paciente/{idPaciente}")]
        public async Task<ActionResult> GetAntecedentePorPaciente(int idPaciente)
        {
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
                    return NoContent();

                return Ok(antecedente);
            }
            catch
            {
                return StatusCode(500, "Error al obtener los antecedentes del paciente.");
            }
        }

        // POST: Crear antecedentes
        [HttpPost]
        public async Task<ActionResult> CrearAntecedente(CrearAntecedentesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

                return CreatedAtAction(nameof(GetAntecedentePorPaciente),
                    new { idPaciente = dto.ID_Paciente },
                    "Antecedentes creados correctamente.");
            }
            catch
            {
                return StatusCode(500, "Error al crear los antecedentes.");
            }
        }

        // PUT: Actualizar antecedentes
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarAntecedente(int id, CrearAntecedentesDTO dto)
        {
            try
            {
                var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
                if (antecedente == null)
                    return NotFound("Antecedente no encontrado.");

                antecedente.Alergias = dto.Alergias;
                antecedente.Enfermedades_Cronicas = dto.Enfermedades_Cronicas;
                antecedente.Observaciones_Generales = dto.Observaciones_Generales;

                await _context.SaveChangesAsync();

                return Ok("Antecedentes actualizados correctamente.");
            }
            catch
            {
                return StatusCode(500, "Error al actualizar los antecedentes.");
            }
        }

        // DELETE: Eliminar antecedentes
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAntecedente(int id)
        {
            try
            {
                var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
                if (antecedente == null)
                    return NotFound("Antecedente no encontrado.");

                _context.AntecedentesMedicos.Remove(antecedente);
                await _context.SaveChangesAsync();

                return Ok("Antecedentes eliminados correctamente.");
            }
            catch
            {
                return StatusCode(500, "Error al eliminar los antecedentes.");
            }
        }
    }
}
