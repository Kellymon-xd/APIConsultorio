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
        private readonly LogEventos log = new LogEventos();

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
            log.setMensaje("Solicitando lista de usuarios...");
            log.informacion();

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

            log.setMensaje($"Total de usuarios encontrados: {usuarios.Count}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, usuarios);
        }

        // ============================================================
        // GET: Obtener usuario por ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(string id)
        {
            log.setMensaje($"Buscando usuario con ID: {id}");
            log.informacion();

            var usuario = await _context.Usuarios
                .Where(x => x.Id_Usuario == id)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                log.setMensaje($"Usuario con ID {id} no encontrado");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Usuario no encontrado");
            }

            return StatusCode(StatusCodes.Status200OK, usuario);
        }

        // ============================================================
        // POST: Crear usuario
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> CrearUsuario(CrearUsuarioDTO dto)
        {
            log.setMensaje($"Intentando crear usuario {dto.Email}");
            log.informacion();

            if (!ModelState.IsValid)
            {
                log.setMensaje("Modelo inválido al crear usuario");
                log.informacion();
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }

            try
            {
                var usuario = new Usuario
                {
                    Id_Usuario = string.Empty,
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

                log.setMensaje($"Usuario {dto.Email} creado correctamente");
                log.informacion();

                return StatusCode(StatusCodes.Status201Created, "Usuario creado correctamente");
            }
            catch (Exception ex)
            {
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al crear usuario");
            }
        }

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            log.setMensaje($"Intento de login para: {dto.Email}");
            log.informacion();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (usuario == null)
            {
                log.setMensaje($"Login fallido, usuario no encontrado: {dto.Email}");
                log.informacion();
                return StatusCode(StatusCodes.Status401Unauthorized, "Contraseña o correo incorrectos");
            }

            if (usuario.Contrasena != HashSHA256(dto.Contrasena))
            {
                log.setMensaje($"Login fallido, contraseña incorrecta: {dto.Email}");
                log.informacion();
                return StatusCode(StatusCodes.Status401Unauthorized, "Contraseña o correo incorrectos");
            }

            log.setMensaje($"Login exitoso: {dto.Email}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, new
            {
                usuario.Id_Usuario,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Id_Rol
            });
        }

        // ============================================================
        // PUT: Actualizar usuario
        // ============================================================
        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarUsuario(string id, ActualizarUsuarioDTO dto)
        {
            log.setMensaje($"Actualizando usuario con ID: {id}");
            log.informacion();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                log.setMensaje($"No se encontró usuario con ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Usuario no encontrado");
            }

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();

            log.setMensaje($"Usuario actualizado correctamente: {id}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, "Usuario actualizado correctamente");
        }

        // ============================================================
        // PUT: Cambiar contraseña
        // ============================================================
        [HttpPut("cambiar-password/{id}")]
        public async Task<ActionResult> CambiarPassword(string id, [FromBody] string nuevaPassword)
        {
            log.setMensaje($"Cambio de contraseña solicitado para usuario {id}");
            log.informacion();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                log.setMensaje($"Usuario no encontrado para cambio de contraseña: {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Usuario no encontrado");
            }

            usuario.Contrasena = HashSHA256(nuevaPassword);
            await _context.SaveChangesAsync();

            log.setMensaje($"Contraseña actualizada para usuario {id}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, "Contraseña actualizada");
        }

        // ============================================================
        // PUT: Bloquear / Desbloquear usuario
        // ============================================================
        [HttpPut("estado/{id}")]
        public async Task<ActionResult> CambiarEstado(string id, [FromBody] bool bloquear)
        {
            log.setMensaje($"Cambiando estado del usuario {id}. Bloquear = {bloquear}");
            log.informacion();

            var actividad = await _context.ActividadUsuarios.FindAsync(id);

            if (actividad == null)
            {
                log.setMensaje($"Actividadusuario no encontrada para ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Actividad no encontrada");
            }

            actividad.Bloqueado = bloquear;
            actividad.Activo = !bloquear;

            await _context.SaveChangesAsync();

            log.setMensaje($"Estado del usuario {id} cambiado a {(bloquear ? "BLOQUEADO" : "ACTIVO")}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, bloquear ? "Usuario bloqueado" : "Usuario activado");
        }

        // ============================================================
        // DELETE
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarUsuario(string id)
        {
            log.setMensaje($"Intentando eliminar usuario {id}");
            log.informacion();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                log.setMensaje($"No se encontró el usuario con ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Usuario no encontrado");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            log.setMensaje($"Usuario {id} eliminado");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, "Usuario eliminado");
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
