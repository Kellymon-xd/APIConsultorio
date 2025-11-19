using Microsoft.AspNetCore.Mvc;
using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.EntityFrameworkCore;

namespace APIConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PacientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/pacientes
        [HttpPost]
        public async Task<IActionResult> Crear(PacienteCreateDto dto)
        {
            var paciente = new Paciente
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Cedula = dto.Cedula,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                ContactoEmergencia = dto.ContactoEmergencia,
                Activo = true
            };

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Paciente registrado", id = paciente.ID_Paciente });
        }

        // GET api/pacientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteResponseDto>>> Listar()
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

            return Ok(pacientes);
        }

        // GET api/pacientes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var p = await _context.Pacientes.FindAsync(id);
            if (p == null) return NotFound("Paciente no encontrado");

            return Ok(p);
        }

        // PUT api/pacientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, PacienteUpdateDto dto)
        {
            var p = await _context.Pacientes.FindAsync(id);
            if (p == null) return NotFound("Paciente no encontrado");

            p.Nombre = dto.Nombre;
            p.Apellido = dto.Apellido;
            p.Telefono = dto.Telefono;
            p.Email = dto.Email;
            p.Direccion = dto.Direccion;
            p.ContactoEmergencia = dto.ContactoEmergencia;
            p.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok("Paciente actualizado");
        }

        // DELETE lógico
        [HttpDelete("{id}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var p = await _context.Pacientes.FindAsync(id);
            if (p == null) return NotFound("Paciente no encontrado");

            p.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Paciente desactivado");
        }
    }
}
