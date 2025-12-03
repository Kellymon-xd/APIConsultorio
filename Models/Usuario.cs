using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("Id_Usuario")]
        [StringLength(8)]
        public string Id_Usuario { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        [Required, StringLength(255)]
        public string Email { get; set; }

        [Required, StringLength(30)]
        public string Cedula { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        [Required, StringLength(128)]
        public string Contrasena { get; set; }

        public byte Id_Rol { get; set; }

        public DateTime Fecha_Registro { get; set; }

        public bool PedirContraseña { get; set; }

        // 🔗 Relaciones

        public ActividadUsuario ActividadUsuario { get; set; }

        public Medico Medico { get; set; }
    }
}