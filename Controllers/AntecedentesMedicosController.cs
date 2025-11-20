using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntecedentesMedicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AntecedentesMedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Listado de antecedentes
        [HttpGet]
        public async Task<ActionResult> GetAntecedentes()
        {
            var lista = await _context.AntecedentesMedicos
                .Include(a => a.Paciente)
                .Select(a => new
                {
                    a.ID_Antecedente,
                    a.ID_Paciente,
                    NombrePaciente = a.Paciente.Nombre + " " + a.Paciente.Apellido,
                    a.Alergias,
                    a.Enfermedades_Cronicas,
                    a.Observaciones_Generales,
                    a.Fecha_Registro
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET: Detalle por ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAntecedente(int id)
        {
            var antecedente = await _context.AntecedentesMedicos
                .Include(a => a.Paciente)
                .FirstOrDefaultAsync(a => a.ID_Antecedente == id);

            if (antecedente == null) return NotFound();
            return Ok(antecedente);
        }

        // POST: Crear antecedentes
        [HttpPost]
        public async Task<ActionResult> CrearAntecedente(CrearAntecedentesDTO dto)
        {
            var antecedente = new AntecedentesMedico
            {
                ID_Paciente = dto.ID_Paciente,
                Alergias = dto.Alergias,
                Enfermedades_Cronicas = dto.Enfermedades_Cronicas,
                Observaciones_Generales = dto.Observaciones_Generales,
                Fecha_Registro = DateTime.Now
            };

            _context.AntecedentesMedicos.Add(antecedente);
            await _context.SaveChangesAsync();

            return Ok("Antecedentes creados correctamente.");
        }

        // PUT: Actualizar antecedentes
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarAntecedente(int id, CrearAntecedentesDTO dto)
        {
            var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
            if (antecedente == null) return NotFound();

            antecedente.Alergias = dto.Alergias;
            antecedente.Enfermedades_Cronicas = dto.Enfermedades_Cronicas;
            antecedente.Observaciones_Generales = dto.Observaciones_Generales;

            await _context.SaveChangesAsync();
            return Ok("Antecedentes actualizados correctamente.");
        }

        // DELETE: Eliminar antecedentes
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarAntecedente(int id)
        {
            var antecedente = await _context.AntecedentesMedicos.FindAsync(id);
            if (antecedente == null) return NotFound();

            _context.AntecedentesMedicos.Remove(antecedente);
            await _context.SaveChangesAsync();
            return Ok("Antecedentes eliminados correctamente.");
        }
    }
}
