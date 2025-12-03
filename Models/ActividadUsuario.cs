using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ACTIVIDAD_USUARIOS")]
    public class ActividadUsuario
    {
        [Key]
        [Column("Id_Usuario")]
        [StringLength(8)]
        public string Id_Usuario { get; set; }

        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
        public int Intentos_Fallidos { get; set; }
        public DateTime? Fecha_Bloqueo { get; set; }
        public DateTime? Ultima_Actividad { get; set; }

        // 🔗 Relación inversa
        public Usuario Usuario { get; set; }
    }
}