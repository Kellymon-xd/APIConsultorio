using ApiConsultorio.Contexts;
using ApiConsultorio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ApiConsultorio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Obtener todos los usuarios
        // ============================================================
        [HttpGet]
        public async Task<ActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new MostrarUsuarioDTO
                {
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Cedula = u.Cedula,
                    Telefono = u.Telefono,
                    IdRol = u.Id_Rol
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // ============================================================
        // GET: Obtener usuario por ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(string id)
        {
            var usuario = await _context.Usuarios
                .Where(x => x.Id_Usuario == id)
                .Select(u => new Usuario
                {
                    Id_Usuario = u.Id_Usuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Cedula = u.Cedula,
                    Telefono = u.Telefono,
                    Id_Rol = u.Id_Rol
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // ============================================================
        // POST: Crear usuario (rol general, NO médico)
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearUsuario(CrearUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = new Usuario
            {
                Id_Usuario = String.Empty,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Cedula = dto.Cedula,
                Telefono = dto.Telefono,
                Contrasena = HashSHA256(dto.Contrasena),
                Id_Rol = dto.IdRol,
                Fecha_Registro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario creado correctamente.");
        }

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado.");

            if (usuario.Contrasena != HashSHA256(dto.Contrasena))
                return Unauthorized("Contraseña incorrecta.");

            return Ok(new
            {
                usuario.Id_Usuario,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Id_Rol
            });
        }

        // ============================================================
        // PUT: Actualizar datos básicos
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarUsuario(string id, ActualizarUsuarioDTO dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();

            return Ok("Usuario actualizado correctamente.");
        }

        // ============================================================
        // PUT: Cambiar contraseña
        // ============================================================
        [HttpPut("cambiar-password/{id}")]
        public async Task<ActionResult> CambiarPassword(string id, [FromBody] string nuevaPassword)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            usuario.Contrasena = HashSHA256(nuevaPassword);

            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada.");
        }

        // ============================================================
        // PUT: Bloquear / Desbloquear usuario
        // ============================================================
        [HttpPut("estado/{id}")]
        public async Task<ActionResult> CambiarEstado(string id, [FromBody] bool bloquear)
        {
            var actividad = await _context.ActividadUsuarios.FindAsync(id);

            if (actividad == null)
                return NotFound();

            actividad.Bloqueado = bloquear;
            actividad.Activo = !bloquear;

            await _context.SaveChangesAsync();

            return Ok(bloquear ? "Usuario bloqueado." : "Usuario activado.");
        }

        // ============================================================
        // DELETE
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarUsuario(string id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario eliminado.");
        }

        // ============================================================
        // HASH
        // ============================================================
        private static string HashSHA256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
