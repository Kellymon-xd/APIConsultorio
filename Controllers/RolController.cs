using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

        public RolController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Obtener todos los roles
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            log.setMensaje("Solicitando lista de roles...");
            log.informacion();

            try
            {
                var roles = await _context.Rol
                    .Select(r => new Rol
                    {
                        Id_Rol = r.Id_Rol,
                        Descripcion_Rol = r.Descripcion_Rol
                    })
                    .ToListAsync();

                log.setMensaje($"Total de roles encontrados: {roles.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, roles);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener roles");
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al obtener roles");
            }
        }

        // ============================================================
        // GET: Obtener rol por ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetById(byte id)
        {
            log.setMensaje($"Buscando rol con ID: {id}");
            log.informacion();

            try
            {
                var rol = await _context.Rol.FindAsync(id);

                if (rol == null)
                {
                    log.setMensaje($"Rol con ID {id} no encontrado");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound, "Rol no encontrado");
                }

                log.setMensaje($"Rol {id} encontrado correctamente");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, new Rol
                {
                    Id_Rol = rol.Id_Rol,
                    Descripcion_Rol = rol.Descripcion_Rol
                });
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al obtener rol con ID {id}");
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }
    }
}
