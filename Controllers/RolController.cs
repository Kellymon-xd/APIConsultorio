using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
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

        public RolController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/rol
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            var roles = await _context.Rol
                .Select(r => new Rol
                {
                    Id_Rol = r.Id_Rol,
                    Descripcion_Rol = r.Descripcion_Rol
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetById(byte id)
        {
            var r = await _context.Rol.FindAsync(id);

            if (r == null)
                return NotFound();

            return Ok(new Rol
            {
                Id_Rol = r.Id_Rol,
                Descripcion_Rol = r.Descripcion_Rol
            });
        }
    }
}
