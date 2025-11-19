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

        public EstadoCitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/estadocita
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadoCitaComboDTO>>> GetEstados()
        {
            var estados = await _context.EstadoCita
                .Select(e => new EstadoCitaComboDTO
                {
                    ID_Estado_Cita = e.ID_Estado_Cita,
                    Descripcion = e.Descripcion
                })
                .ToListAsync();

            return Ok(estados);
        }
    }
}
