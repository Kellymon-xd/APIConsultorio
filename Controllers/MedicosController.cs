using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Listar médicos con información del usuario
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> GetMedicos()
        {
            var lista = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidad)
                .Include(m => m.Contrato)
                .Select(m => new
                {
                    m.ID_Medico,
                    m.Id_Usuario,
                    m.Usuario.Nombre,
                    m.Usuario.Apellido,
                    m.Usuario.Email,
                    Especialidad = m.Especialidad.Nombre_Especialidad,
                    Contrato = m.Contrato.Descripcion,
                    m.Horario_Atencion,
                    m.Telefono_Consulta,
                    m.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ============================================================
        // GET: Obtener médico por ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult> GetMedico(int id)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidad)
                .Include(m => m.Contrato)
                .FirstOrDefaultAsync(m => m.ID_Medico == id);

            if (medico == null)
                return NotFound();

            return Ok(medico);
        }

        // ============================================================
        // POST: Crear médico (crea usuario + médico)
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearMedico([FromBody] CrearMedicoDTO dto)
        {
            // 1. Crear usuario (Id_Usuario lo genera el trigger)
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Cedula = dto.Cedula,
                Telefono = dto.Telefono,
                Contrasena = HashSHA256(dto.Contrasena),
                IdRol = 2 // MEDICO
            };

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync(); // Aquí ya tenemos el Id_Usuario generado

            // 2. Crear médico
            var medico = new Medico
            {
                Id_Usuario = usuario.IdUsuario,
                ID_Especialidad = dto.ID_Especialidad,
                ID_Contrato = dto.ID_Contrato,
                Horario_Atencion = dto.Horario_Atencion,
                Telefono_Consulta = dto.Telefono_Consulta,
                Activo = true
            };

            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Médico creado correctamente",
                usuario.IdUsuario,
                medico.ID_Medico
            });
        }

        // ============================================================
        // PUT: Editar médico
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> EditarMedico(int id, [FromBody] ActualizarMedicoDTO dto)
        {
            var medico = await _context.Medicos.FindAsync(id);

            if (medico == null)
                return NotFound();

            medico.ID_Especialidad = dto.ID_Especialidad;
            medico.ID_Contrato = dto.ID_Contrato;
            medico.Horario_Atencion = dto.Horario_Atencion;
            medico.Telefono_Consulta = dto.Telefono_Consulta;

            await _context.SaveChangesAsync();

            return Ok("Médico actualizado.");
        }

        // ============================================================
        // DELETE: Eliminar médico
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarMedico(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);

            if (medico == null)
                return NotFound();

            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();

            return Ok("Médico eliminado.");
        }


        // ===========================
        // HASH SHA256
        // ===========================
        private static string HashSHA256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        [HttpGet("detalle/{id}")]
        public async Task<ActionResult> GetDetalleMedico(int id)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidad)
                .Include(m => m.Contrato)
                .Where(m => m.ID_Medico == id)
                .Select(m => new
                {
                    m.ID_Medico,

                    Usuario = new
                    {
                        m.Usuario.IdUsuario,
                        m.Usuario.Nombre,
                        m.Usuario.Apellido,
                        NombreCompleto = m.Usuario.Nombre + " " + m.Usuario.Apellido,
                        m.Usuario.Email,
                        m.Usuario.Cedula,
                        m.Usuario.Telefono
                    },

                    Especialidad = new
                    {
                        m.Especialidad.ID_Especialidad,
                        m.Especialidad.Nombre_Especialidad,
                        m.Especialidad.Descripcion
                    },

                    Contrato = new
                    {
                        m.Contrato.ID_Contrato,
                        m.Contrato.Descripcion
                    },

                    m.Horario_Atencion,
                    m.Telefono_Consulta,
                    m.Activo
                })
                .FirstOrDefaultAsync();

            if (medico == null)
                return NotFound("Médico no encontrado.");

            return Ok(medico);
        }

    }
}