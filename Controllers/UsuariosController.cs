using ApiConsultorio.Contexts;
using ApiConsultorio.DTOs;
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

                var usuarioCreado = await _context.Usuarios
                    .Where(u => u.Email == dto.Email)
                    .Select(u => new { u.Id_Usuario })
                    .FirstOrDefaultAsync();

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Usuario creado correctamente",
                    idUsuario = usuarioCreado?.Id_Usuario
                });
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

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (usuario == null)
                return Unauthorized("Correo o contraseña incorrectos");

            var actividad = await _context.ActividadUsuarios
                .FirstOrDefaultAsync(a => a.Id_Usuario == usuario.Id_Usuario);

            if (actividad == null)
                return StatusCode(500, "Error interno: actividad no encontrada");

            // -----------------------------
            // VALIDAR ESTADO DEL USUARIO
            // -----------------------------
            if (!actividad.Activo)
                return StatusCode(403, "Usuario inactivo. Contacte al administrador.");

            if (actividad.Bloqueado)
                return StatusCode(423, "Usuario bloqueado temporalmente.");

            // -----------------------------
            // VALIDAR CONTRASEÑA
            // -----------------------------
            if (usuario.Contrasena != HashSHA256(dto.Contrasena))
            {
                actividad.Intentos_Fallidos++;

                if (actividad.Intentos_Fallidos >= 3)
                {
                    actividad.Bloqueado = true;
                    actividad.Fecha_Bloqueo = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return StatusCode(423, "Usuario bloqueado por intentos fallidos.");
                }

                await _context.SaveChangesAsync();
                return Unauthorized("Correo o contraseña incorrectos");
            }

            // -----------------------------
            // LOGIN EXITOSO
            // -----------------------------
            actividad.Intentos_Fallidos = 0;
            actividad.Bloqueado = false;
            actividad.Ultima_Actividad = DateTime.Now;

            await _context.SaveChangesAsync();

            int? idMedico = null;

            if (usuario.Id_Rol == 2)
            {
                var medico = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Id_Usuario == usuario.Id_Usuario);

                if (medico != null)
                    idMedico = medico.ID_Medico;
            }

            return Ok(new
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
        // PATCH: Bloquear / Desbloquear usuario
        // ============================================================
        [HttpPatch("{id}/bloqueado")]
        public async Task<ActionResult> CambiarBloqueado(string id, [FromBody] bool bloquear)
        {
            var actividad = await _context.ActividadUsuarios.FindAsync(id);
            if (actividad == null) return NotFound("Actividad no encontrada");

            actividad.Bloqueado = bloquear;
            await _context.SaveChangesAsync();

            return Ok(bloquear ? "Usuario bloqueado" : "Usuario desbloqueado");
        }

        // ============================================================
        // PATCH: Activar / Inactivar usuario
        // ============================================================

        [HttpPatch("{id}/activo")]
        public async Task<ActionResult> CambiarActivo(string id, [FromBody] bool activo)
        {
            var actividad = await _context.ActividadUsuarios.FindAsync(id);
            if (actividad == null) return NotFound("Actividad no encontrada");

            actividad.Activo = activo;
            await _context.SaveChangesAsync();

            return Ok(activo ? "Usuario activado" : "Usuario inactivado");
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
                log.setMensaje($"No existe el usuario con ID {id}");
                log.informacion();
                return StatusCode(StatusCodes.Status404NotFound, new
                {
                    message = "Usuario no encontrado",
                    code = 404
                });
            }

            // Si es médico (Id_Rol = 2)
            if (usuario.Id_Rol == 2)
            {
                log.setMensaje($"Usuario {id} es médico. Verificando dependencias...");
                log.informacion();

                // Obtener el ID_Medico asociado
                var medico = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Id_Usuario == id);

                if (medico == null)
                {
                    log.setMensaje($"Error: el usuario {id} tiene rol médico pero no tiene entrada en MEDICOS.");
                    log.error();
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        message = "Inconsistencia en datos: médico sin registro en tabla MEDICOS",
                        code = 500
                    });
                }

                // Verificar si el médico tiene citas
                bool tieneCitas = await _context.Citas
                    .AnyAsync(c => c.ID_Medico == medico.ID_Medico);

                if (tieneCitas)
                {
                    log.setMensaje($"No se puede eliminar el médico {id}, tiene citas registradas.");
                    log.informacion();

                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        message = "No se puede eliminar el médico: tiene citas registradas",
                        code = 409
                    });
                }

                // Si no tiene citas → permitir eliminación total
                log.setMensaje($"Médico {id} sin citas. Eliminando usuario + actividad + médico...");
                log.informacion();
            }
            else
            {
                log.setMensaje($"Usuario {id} NO es médico. Eliminación directa.");
                log.informacion();
            }

            // === Eliminación ===
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            log.setMensaje($"Usuario {id} eliminado correctamente.");
            log.informacion();

            return StatusCode(StatusCodes.Status200OK, new
            {
                message = "Usuario eliminado correctamente",
                code = 200
            });
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
