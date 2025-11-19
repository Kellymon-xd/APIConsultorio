using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiConsultorio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspecialidadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) COMBOBOX → SOLO ID + NOMBRE
        // GET: api/Especialidades/combo
        // ============================================================
        [HttpGet("combo")]
        public async Task<ActionResult<IEnumerable<EspecialidadComboDTO>>> GetCombo()
        {
            var result = await _context.Especialidades
                .Select(e => new EspecialidadComboDTO
                {
                    ID_Especialidad = e.ID_Especialidad,
                    Nombre_Especialidad = e.Nombre_Especialidad
                })
                .ToListAsync();

            return Ok(result);
        }

        // ============================================================
        // 2) LISTAR TODAS
        // GET: api/Especialidades
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspecialidadDTO>>> GetAll()
        {
            var lista = await _context.Especialidades
                .Select(e => new EspecialidadDTO
                {
                    ID_Especialidad = e.ID_Especialidad,
                    Nombre_Especialidad = e.Nombre_Especialidad,
                    Descripcion = e.Descripcion
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ============================================================
        // 3) OBTENER UNA POR ID (editar)
        // GET: api/Especialidades/5
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<EspecialidadDTO>> GetById(int id)
        {
            var e = await _context.Especialidades.FindAsync(id);

            if (e == null)
                return NotFound();

            return new EspecialidadDTO
            {
                ID_Especialidad = e.ID_Especialidad,
                Nombre_Especialidad = e.Nombre_Especialidad,
                Descripcion = e.Descripcion
            };
        }

        // ============================================================
        // 4) CREAR
        // POST: api/Especialidades
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> Create(EspecialidadDTO dto)
        {
            var especialidad = new Especialidad
            {
                Nombre_Especialidad = dto.Nombre_Especialidad,
                Descripcion = dto.Descripcion
            };

            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Especialidad creada correctamente" });
        }

        // ============================================================
        // 5) EDITAR
        // PUT: api/Especialidades/5
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, EspecialidadDTO dto)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);

            if (especialidad == null)
                return NotFound();

            especialidad.Nombre_Especialidad = dto.Nombre_Especialidad;
            especialidad.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Especialidad actualizada correctamente" });
        }

        // ============================================================
        // 6) ELIMINAR
        // DELETE: api/Especialidades/5
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);

            if (especialidad == null)
                return NotFound();

            _context.Especialidades.Remove(especialidad);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Especialidad eliminada correctamente" });
        }
    }
}
