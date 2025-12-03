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
                .Include(m => m.Usuario)
                    .ThenInclude(u => u.ActividadUsuario)
                .Include(m => m.Especialidad)
                .Include(m => m.Contrato)
                .Where(m => m.Usuario.ActividadUsuario.Activo == true &&
                            m.Usuario.ActividadUsuario.Bloqueado == false)
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
                    m.Telefono_Consulta
                })
                .ToListAsync();

            log.setMensaje($"Total de médicos encontrados: {lista.Count}");
            log.informacion();

            return Ok(lista);
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
        // POST: Crear médico
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearMedico([FromBody] CrearMedicoDTO dto)
        {
            log.setMensaje($"Intentando crear médico para ID_Usuario {dto.Id_Usuario}");
            log.informacion();

            try
            {
                // Primero verificar que el usuario exista y tenga rol 2
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id_Usuario == dto.Id_Usuario);

                if (usuario == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                        "El usuario especificado no existe.");
                }

                if (usuario.Id_Rol != 2)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        "El usuario no tiene rol de médico.");
                }

                // Crear médico
                var medico = new Medico
                {
                    Id_Usuario = dto.Id_Usuario,
                    ID_Especialidad = dto.ID_Especialidad,
                    ID_Contrato = dto.ID_Contrato,
                    Horario_Atencion = dto.Horario_Atencion,
                    Telefono_Consulta = dto.Telefono_Consulta,
                };

                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                log.setMensaje($"Médico creado con ID {medico.ID_Medico}");
                log.informacion();

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Médico creado correctamente",
                    medico.ID_Medico
                });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear médico");
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error al crear médico");
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
        // GET: Listar médicos
        // ============================================================
        [HttpGet("combo")]
        public async Task<ActionResult<IEnumerable<MedicoComboDTO>>> GetCombo()
        {
            log.setMensaje("Solicitando lista para combo de médicos...");
            log.informacion();

            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                    .ThenInclude(u => u.ActividadUsuario)
                .Where(m => m.Usuario.ActividadUsuario.Activo == true)
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
