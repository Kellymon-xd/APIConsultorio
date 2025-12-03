using ApiConsultorio;
using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

        public PacientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/pacientes
        [HttpPost]
        public async Task<IActionResult> Crear(PacienteCreateDto dto)
        {
            try
            {
                var paciente = new Paciente
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Cedula = dto.Cedula,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Sexo = dto.Sexo,
                    Fecha_Nacimiento = dto.Fecha_Nacimiento,
                    Direccion = dto.Direccion,
                    ContactoEmergencia = dto.ContactoEmergencia,
                    Activo = true
                };

                _context.Pacientes.Add(paciente);
                await _context.SaveChangesAsync();

                log.setMensaje($"Paciente creado con ID: {paciente.ID_Paciente}");
                log.informacion();

                return Ok(new { mensaje = "Paciente registrado", id = paciente.ID_Paciente });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear paciente");
                log.informacion(ex);
                return StatusCode(500, "Error al crear el paciente");
            }
        }

        // GET api/pacientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteResponseDto>>> Listar()
        {
            try
            {
                var pacientes = await _context.Pacientes
                    .Select(p => new PacienteResponseDto
                    {
                        Id_Paciente = p.ID_Paciente,
                        NombreCompleto = p.Nombre + " " + p.Apellido,
                        Cedula = p.Cedula,
                        Telefono = p.Telefono,
                        Email = p.Email,
                        Activo = p.Activo
                    })
                    .ToListAsync();

                log.setMensaje($"Total de pacientes encontrados: {pacientes.Count}");
                log.informacion();

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al listar pacientes");
                log.informacion(ex);
                return StatusCode(500, "Error al obtener la lista de pacientes");
            }
        }

        // GET api/pacientes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            try
            {
                var p = await _context.Pacientes.FindAsync(id);

                if (p == null)
                {
                    log.setMensaje($"Paciente con ID {id} no encontrado");
                    log.informacion();
                    return NotFound("Paciente no encontrado");
                }

                log.setMensaje($"Paciente obtenido: {p.ID_Paciente}");
                log.informacion();

                return Ok(p);
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al obtener paciente con ID {id}");
                log.informacion(ex);
                return StatusCode(500, "Error al obtener el paciente");
            }
        }

        // PUT api/pacientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, PacienteUpdateDto dto)
        {
            try
            {
                var p = await _context.Pacientes.FindAsync(id);
                if (p == null)
                {
                    log.setMensaje($"Paciente con ID {id} no encontrado para actualizar");
                    log.informacion();
                    return NotFound("Paciente no encontrado");
                }

                p.Nombre = dto.Nombre;
                p.Apellido = dto.Apellido;
                p.Telefono = dto.Telefono;
                p.Email = dto.Email;
                p.Direccion = dto.Direccion;
                p.ContactoEmergencia = dto.ContactoEmergencia;
                p.Activo = dto.Activo;

                await _context.SaveChangesAsync();

                log.setMensaje($"Paciente actualizado ID {id}");
                log.informacion();

                return Ok("Paciente actualizado");
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al actualizar paciente con ID {id}");
                log.informacion(ex);
                return StatusCode(500, "Error al actualizar el paciente");
            }
        }

        // DELETE lógico
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                var p = await _context.Pacientes.FindAsync(id);
                if (p == null)
                {
                    log.setMensaje($"Paciente con ID {id} no encontrado para desactivar");
                    log.informacion();
                    return NotFound("Paciente no encontrado");
                }

                p.Activo = false;
                await _context.SaveChangesAsync();

                log.setMensaje($"Paciente desactivado ID {id}");
                log.informacion();

                return Ok("Paciente desactivado");
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al desactivar paciente con ID {id}");
                log.informacion(ex);
                return StatusCode(500, "Error al desactivar el paciente");
            }
        }

        // GET api/pacientes/combo
        [HttpGet("combo")]
        public async Task<ActionResult<IEnumerable<PacienteResponseDto>>> GetCombo()
        {
            try
            {
                var pacientes = await _context.Pacientes
                    .Select(p => new PacienteComboDTO
                    {
                        Id_Paciente = p.ID_Paciente,
                        NombreCompleto = p.Nombre + " " + p.Apellido,
                        Cedula = p.Cedula
                    })
                    .ToListAsync();

                log.setMensaje($"Total de pacientes en combo: {pacientes.Count}");
                log.informacion();

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener combo de pacientes");
                log.informacion(ex);
                return StatusCode(500, "Error al obtener el combo de pacientes");
            }
        }
    }
}
