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
        private readonly LogEventos log = new LogEventos();

        public MedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Listar médicos
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> GetMedicos()
        {
            log.setMensaje("Solicitando lista de médicos...");
            log.informacion();

            var lista = await _context.Medicos
                .Where(m => m.Activo == true)
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

            log.setMensaje($"Total de médicos encontrados: {lista.Count}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // ============================================================
        // GET: Médico por ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult> GetMedico(int id)
        {
            log.setMensaje($"Buscando médico con ID: {id}");
            log.informacion();

            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Especialidad)
                .Include(m => m.Contrato)
                .FirstOrDefaultAsync(m => m.ID_Medico == id);

            if (medico == null)
            {
                log.setMensaje($"Médico con ID {id} no encontrado");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Médico no encontrado");
            }

            return StatusCode(StatusCodes.Status200OK, medico);
        }

        // ============================================================
        // POST: Crear médico + usuario
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearMedico([FromBody] CrearMedicoDTO dto)
        {
            log.setMensaje($"Intentando crear médico con correo {dto.Email}");
            log.informacion();

            try
            {
                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Email = dto.Email,
                    Cedula = dto.Cedula,
                    Telefono = dto.Telefono,
                    Contrasena = HashSHA256(dto.Contrasena),
                    Id_Rol = 2
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var medico = new Medico
                {
                    Id_Usuario = usuario.Id_Usuario,
                    ID_Especialidad = dto.ID_Especialidad,
                    ID_Contrato = dto.ID_Contrato,
                    Horario_Atencion = dto.Horario_Atencion,
                    Telefono_Consulta = dto.Telefono_Consulta,
                    Activo = true
                };

                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                log.setMensaje($"Médico creado correctamente con ID {medico.ID_Medico}");
                log.informacion();

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Médico creado correctamente",
                    usuario.Id_Usuario,
                    medico.ID_Medico
                });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear médico");
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear médico");
            }
        }

        // ============================================================
        // PUT: Actualizar médico
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> EditarMedico(int id, [FromBody] ActualizarMedicoDTO dto)
        {
            log.setMensaje($"Actualizando médico con ID {id}");
            log.informacion();

            var medico = await _context.Medicos.FindAsync(id);

            if (medico == null)
            {
                log.setMensaje($"No se encontró médico con ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Médico no encontrado");
            }

            medico.ID_Especialidad = dto.ID_Especialidad;
            medico.ID_Contrato = dto.ID_Contrato;
            medico.Horario_Atencion = dto.Horario_Atencion;
            medico.Telefono_Consulta = dto.Telefono_Consulta;

            await _context.SaveChangesAsync();

            log.setMensaje($"Médico {id} actualizado correctamente");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, "Médico actualizado correctamente");
        }

        // ============================================================
        // DELETE: Eliminar médico
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarMedico(int id)
        {
            log.setMensaje($"Intentando eliminar médico {id}");
            log.informacion();

            var medico = await _context.Medicos.FindAsync(id);

            if (medico == null)
            {
                log.setMensaje($"No se encontró el médico con ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Médico no encontrado");
            }

            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();

            log.setMensaje($"Médico {id} eliminado");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, "Médico eliminado");
        }


        // ============================================================
        // HASH 
        // ============================================================
        private static string HashSHA256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // ============================================================
        // GET: Listar médicos
        // ============================================================
        [HttpGet("combo")]
        public async Task<ActionResult<IEnumerable<MedicoComboDTO>>> GetCombo()
        {
            log.setMensaje("Solicitando lista para combo de médicos...");
            log.informacion();

            var medicos = await _context.Medicos
                .Where(m => m.Activo == true)
                .Include(m => m.Usuario)
                .Select(m => new MedicoComboDTO
                {
                    Id_Medico = m.ID_Medico,
                    NombreCompleto = m.Usuario.Nombre + " " + m.Usuario.Apellido,
                    Cedula = m.Usuario.Cedula
                })
                .ToListAsync();

            log.setMensaje($"Total de médicos encontrados (combo): {medicos.Count}");
            log.informacion();

            return Ok(medicos);
        }
    }
}
