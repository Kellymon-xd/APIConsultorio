using System.ComponentModel.DataAnnotations;

namespace ApiConsultorio.DTOs
{
    public class PacienteCreateDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public string? Sexo { get; set; }
        public string? Direccion { get; set; }
        public string? ContactoEmergencia { get; set; }
    }
}