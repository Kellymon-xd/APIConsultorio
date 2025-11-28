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
    public class TipoContratoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoContratoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/tipocontrato
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoContratoComboDTO>>> GetTipos()
        {
            var tipos = await _context.TipoContrato
                .Select(t => new TipoContratoComboDTO
                {
                    ID_Contrato = t.ID_Contrato,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

            return Ok(tipos);
        }

        // GET: api/tipocontrato/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoContratoComboDTO>> GetTipoContrato(int id)
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
                return NotFound(new { mensaje = "Tipo de contrato no encontrado" });

            return Ok(tipo);
        }

    }
}
