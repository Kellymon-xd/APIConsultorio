using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoContratoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

        public TipoContratoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/tipocontrato
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoContratoComboDTO>>> GetTipos()
        {
            log.setMensaje("Solicitando lista de tipos de contrato...");
            log.informacion();

            try
            {
                var tipos = await _context.TipoContrato
                    .Select(t => new TipoContratoComboDTO
                    {
                        ID_Contrato = t.ID_Contrato,
                        Descripcion = t.Descripcion
                    })
                    .ToListAsync();

                log.setMensaje($"Total de tipos de contrato encontrados: {tipos.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, tipos);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener tipos de contrato");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

        // ============================================================
        // GET: api/tipocontrato/{id}
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoContratoComboDTO>> GetTipoContrato(int id)
        {
            log.setMensaje($"Buscando tipo de contrato con ID: {id}");
            log.informacion();

            try
            {
                var tipo = await _context.TipoContrato
                    .Where(t => t.ID_Contrato == id)
                    .Select(t => new TipoContratoComboDTO
                    {
                        ID_Contrato = t.ID_Contrato,
                        Descripcion = t.Descripcion
                    })
                    .FirstOrDefaultAsync();

                if (tipo == null)
                {
                    log.setMensaje($"Tipo de contrato con ID {id} no encontrado");
                    log.informacion();

                    return StatusCode(StatusCodes.Status404NotFound,
                        new { mensaje = "Tipo de contrato no encontrado" });
                }

                log.setMensaje($"Tipo de contrato {id} encontrado correctamente");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, tipo);
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al obtener tipo de contrato con ID {id}");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }

    }
}