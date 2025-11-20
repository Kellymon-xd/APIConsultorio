using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ATENCION_MEDICA")]
    public class AtencionMedica
    {
        [Key]
        public int ID_Atencion { get; set; }

        public int ID_Cita { get; set; }

        public DateTime Fecha_Atencion { get; set; }

        [StringLength(300)]
        public string Motivo_Consulta { get; set; }

        [StringLength(300)]
        public string? Diagnostico { get; set; }

        [StringLength(400)]
        public string? Observaciones { get; set; }

        [ForeignKey("ID_Cita")]
        public virtual Cita Cita { get; set; }
    }
}