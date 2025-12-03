using System.ComponentModel.DataAnnotations;

namespace ApiConsultorio.DTOs
{
    public class PacienteUpdateDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string? Direccion { get; set; }
        public string? ContactoEmergencia { get; set; }
        public bool Activo { get; set; }
    }
}