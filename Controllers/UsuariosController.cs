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

            var usuarios = await (
                from u in _context.Usuarios
                join a in _context.ActividadUsuarios
                    on u.Id_Usuario equals a.Id_Usuario

                select new MostrarUsuarioDTO
                {
                    Id_Usuario = u.Id_Usuario,
                    Cedula=u.Cedula,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Id_Rol = u.Id_Rol,
                    Activo = a.Activo,
                    Bloqueado = a.Bloqueado
                }
            ).ToListAsync();

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
            try
            {
                log.setMensaje($"Buscando usuario con ID: {id}");
                log.informacion();

                var usuario = await _context.Usuarios
                    .Where(u => u.Id_Usuario == id)
                    .Select(u => new
                    {
                        u.Id_Usuario,
                        u.Nombre,
                        u.Apellido,
                        u.Email,
                        u.Cedula,
                        u.Id_Rol,
                        u.Telefono,
                        u.Fecha_Registro
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    log.setMensaje($"Usuario con ID {id} no encontrado");
                    log.informacion();
                    return StatusCode(StatusCodes.Status404NotFound, "Usuario no encontrado");
                }

                var result = new Dictionary<string, object>
                {
                    ["id_Usuario"] = usuario.Id_Usuario,
                    ["nombre"] = usuario.Nombre,
                    ["apellido"] = usuario.Apellido,
                    ["email"] = usuario.Email,
                    ["cedula"] = usuario.Cedula,
                    ["id_Rol"] = usuario.Id_Rol,
                    ["telefono"] = usuario.Telefono,
                    ["fecha_Registro"] = usuario.Fecha_Registro
                };

                // Si es médico, agregar los campos de médico
                if (usuario.Id_Rol == 2)
                {
                    var medico = await _context.Medicos
                        .Where(m => m.Id_Usuario == id)
                        .Select(m => new
                        {
                            m.ID_Medico,
                            m.ID_Especialidad,
                            m.ID_Contrato,
                            m.Horario_Atencion,
                            m.Telefono_Consulta
                        })
                        .FirstOrDefaultAsync();

                    if (medico != null)
                    {
                        result["ID_Medico"] = medico.ID_Medico;
                        result["ID_Especialidad"] = medico.ID_Especialidad;
                        result["ID_Contrato"] = medico.ID_Contrato;
                        result["Horario_Atencion"] = medico.Horario_Atencion;
                        result["Telefono_Consulta"] = medico.Telefono_Consulta;
                    }
                }

                log.setMensaje($"Devolviendo datos del usuario {id}");
                log.informacion();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            catch (Exception ex)
            {
                log.informacion(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor.");
            }
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
                log.setMensaje($"Login fallido, contraseña incorrecta: {HashSHA256(dto.Contrasena)}");
                log.informacion();
                return StatusCode(StatusCodes.Status401Unauthorized, "Contraseña o correo incorrectos");
            }

            log.setMensaje($"Login exitoso: {dto.Email}");
            log.informacion();

            // ======================================
            // OBTENER ID MÉDICO (solo si es médico)
            // ======================================
            int? idMedico = null;

            if (usuario.Id_Rol == 2)
            {
                var medico = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Id_Usuario == usuario.Id_Usuario);

                if (medico != null)
                    idMedico = medico.ID_Medico;
            }

            return StatusCode(StatusCodes.Status200OK, new
            {
                usuario.Id_Usuario,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Id_Rol,
                Id_Medico = idMedico
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
        // PUT: Activar / Inactivar usuario
        // ============================================================
        [HttpPut("activo/{id}")]
        public async Task<ActionResult> CambiarActivo(string id, [FromBody] bool activo)
        {
            log.setMensaje($"Cambiando estado ACTIVO del usuario {id}. Activo = {activo}");
            log.informacion();

            var actividad = await _context.ActividadUsuarios.FindAsync(id);

            if (actividad == null)
            {
                log.setMensaje($"ActividadUsuario no encontrada para ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, "Actividad no encontrada");
            }

            actividad.Activo = activo;

            await _context.SaveChangesAsync();

            log.setMensaje($"Usuario {id} ahora está {(activo ? "ACTIVO" : "INACTIVO")}");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK,
                activo ? "Usuario activado" : "Usuario inactivado");
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
