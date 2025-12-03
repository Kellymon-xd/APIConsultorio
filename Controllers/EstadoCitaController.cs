using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoCitaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

        public EstadoCitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/estadocita
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadoCitaComboDTO>>> GetEstados()
        {
            log.setMensaje("Solicitando lista de estados de cita...");
            log.informacion();

            try
            {
                var estados = await _context.EstadoCita
                    .Select(e => new EstadoCitaComboDTO
                    {
                        ID_Estado_Cita = e.ID_Estado_Cita,
                        Descripcion = e.Descripcion
                    })
                    .ToListAsync();

                log.setMensaje($"Total de estados de cita encontrados: {estados.Count}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, estados);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener los estados de cita");
                log.informacion(ex);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor.");
            }
        }
    }
}
