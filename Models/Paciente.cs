using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("PACIENTES")]
    public class Paciente
    {
        [Key]
        public int ID_Paciente { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [StringLength(30)]
        public string Cedula { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        public DateTime Fecha_Nacimiento { get; set; }

        [StringLength(10)]
        public string? Sexo { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? ContactoEmergencia { get; set; }

        public bool Activo { get; set; }
    }
}