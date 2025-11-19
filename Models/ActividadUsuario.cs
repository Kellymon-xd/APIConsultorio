using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ACTIVIDAD_USUARIOS")]
    public class ActividadUsuario
    {
        [Key]
        [StringLength(8)]
        public string IdUsuario { get; set; }

        public bool Activo { get; set; }

        public bool Bloqueado { get; set; }

        public int IntentosFallidos { get; set; }

        public DateTime? FechaBloqueo { get; set; }

        public DateTime? UltimaActividad { get; set; }
    }
}
