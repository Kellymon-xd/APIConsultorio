using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("CITAS")]
    public class Cita
    {
        [Key]
        public int ID_Cita { get; set; }

        public int ID_Paciente { get; set; }
        public int ID_Medico { get; set; }
        public DateTime Fecha_Cita { get; set; }
        public TimeSpan Hora_Cita { get; set; }
        public int ID_Estado_Cita { get; set; }

        // Navegación
        [ForeignKey("ID_Paciente")]
        public virtual Paciente Paciente { get; set; }

        [ForeignKey("ID_Medico")]
        public virtual Medico Medico { get; set; }

        [ForeignKey("ID_Estado_Cita")]
        public virtual EstadoCita EstadoCita { get; set; }

        public AtencionMedica AtencionMedica { get; set; }
    }
}