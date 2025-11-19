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
    }
}
