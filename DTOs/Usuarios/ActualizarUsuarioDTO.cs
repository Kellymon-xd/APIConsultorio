using System.ComponentModel.DataAnnotations;

namespace ApiConsultorio.DTOs
{
    public class ActualizarUsuarioDTO
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string? Telefono { get; set; }
    }
}