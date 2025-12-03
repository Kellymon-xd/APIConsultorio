using System.ComponentModel.DataAnnotations;

namespace ApiConsultorio.DTOs
{
    public class CrearUsuarioDTO
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string Cedula { get; set; }
        public string? Telefono { get; set; }
        public string Contrasena { get; set; }
        public byte IdRol { get; set; }   // 1=A, 2=M, 3=S
    }
}