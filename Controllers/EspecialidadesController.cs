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
    public class EspecialidadesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LogEventos log = new LogEventos();

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
            log.setMensaje("Solicitando especialidades para combobox...");
            log.informacion();
            try
            {
                var result = await _context.Especialidades
                    .Select(e => new EspecialidadComboDTO
                    {
                        ID_Especialidad = e.ID_Especialidad,
                        Nombre_Especialidad = e.Nombre_Especialidad
                    })
                    .ToListAsync();
                log.setMensaje($"Total de tipos de contrato encontrados: {result.Count}");
                log.informacion();

                return Ok(result);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener lista de especialidades");
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al obtener el combo de especialidades", error = ex.Message });
            }
        }

        // ============================================================
        // 2) LISTAR TODAS
        // GET: api/Especialidades
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspecialidadDTO>>> GetAll()
        {
            log.setMensaje("Solicitando lista de especialidades...");
            log.informacion();
            try
            {
                var lista = await _context.Especialidades
                    .Select(e => new EspecialidadDTO
                    {
                        ID_Especialidad = e.ID_Especialidad,
                        Nombre_Especialidad = e.Nombre_Especialidad,
                        Descripcion = e.Descripcion
                    })
                    .ToListAsync();
                log.setMensaje($"Total de tipos de contrato encontrados: {lista.Count}");
                log.informacion();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener lista de especialidades");
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al obtener la lista de especialidades", error = ex.Message });
            }
        }

        // ============================================================
        // 3) OBTENER UNA POR ID
        // GET: api/Especialidades/5
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<EspecialidadDTO>> GetById(int id)
        {
            log.setMensaje($"Buscando especialidad con id {id}");
            log.informacion();
            try
            {
                var e = await _context.Especialidades.FindAsync(id);

                if (e == null)
                {
                    log.setMensaje($"No se encontro especialidad con id: {id}");
                    log.informacion();
                    return NotFound(new { mensaje = "No se encontró la especialidad" });
                }

                log.setMensaje($"Especialidad con id: {id} encontrada: {e.Nombre_Especialidad}");
                log.informacion();
                return Ok(new EspecialidadDTO
                {
                    ID_Especialidad = e.ID_Especialidad,
                    Nombre_Especialidad = e.Nombre_Especialidad,
                    Descripcion = e.Descripcion
                }); 
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al obtener especialidad con id:"+id);
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al obtener la especialidad", error = ex.Message });
            }
        }

        // ============================================================
        // 4) CREAR
        // POST: api/Especialidades
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> Create(EspecialidadDTO dto)
        {
            log.setMensaje($"Creando especialidad...");
            log.informacion();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre_Especialidad))
                {
                    log.setMensaje($"Error al crear especialidad: El nombre esta en blanco");
                    log.informacion();
                    return BadRequest(new { mensaje = "El nombre de la especialidad es obligatorio" });
                }

                var especialidad = new Especialidad
                {
                    Nombre_Especialidad = dto.Nombre_Especialidad,
                    Descripcion = dto.Descripcion
                };

                _context.Especialidades.Add(especialidad);
                await _context.SaveChangesAsync();

                log.setMensaje($"Especialidad creada correctamente");
                log.informacion();

                return Ok(new { mensaje = "Especialidad creada correctamente" });
            }
            catch (Exception ex)
            {
                log.setMensaje("Error al crear especialidad");
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al crear la especialidad", error = ex.Message });
            }
        }

        // ============================================================
        // 5) EDITAR
        // PUT: api/Especialidades/5
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, EspecialidadDTO dto)
        {
            log.setMensaje($"Actualizando especialidad con id: {id}");
            log.informacion();
            try
            {
                var especialidad = await _context.Especialidades.FindAsync(id);

                if (especialidad == null)
                {
                    log.setMensaje($"No se encontro especialidad con id: {id}");
                    log.informacion();
                    return NotFound(new { mensaje = "No se encontró la especialidad" });
                }

                especialidad.Nombre_Especialidad = dto.Nombre_Especialidad;
                especialidad.Descripcion = dto.Descripcion;

                await _context.SaveChangesAsync();

                log.setMensaje($"Especialidad con id: {id}, actualizada correctamente");
                log.informacion();

                return Ok(new { mensaje = "Especialidad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al actualizar especialidad con id: {id}");
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al actualizar la especialidad", error = ex.Message });
            }
        }

        // ============================================================
        // 6) ELIMINAR
        // DELETE: api/Especialidades/5
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var especialidad = await _context.Especialidades.FindAsync(id);

                if (especialidad == null)
                {
                    log.setMensaje($"Especialidad con id: {id}, no existe, no se puede eliminar");
                    log.informacion();
                    return NotFound(new { mensaje = "No se encontró la especialidad" });
                }

                _context.Especialidades.Remove(especialidad);
                await _context.SaveChangesAsync();

                log.setMensaje($"Especialidad con id: {id}, eliminada correctamente");
                log.informacion();

                return Ok(new { mensaje = "Especialidad eliminada correctamente" });
            }
            catch (Exception ex)
            {
                log.setMensaje($"Error al eliminar especialidad con id: {id}");
                log.informacion(ex);
                return StatusCode(500, new { mensaje = "Error al eliminar la especialidad", error = ex.Message });
            }
        }
    }
}