using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("MEDICOS")]
    public class Medico
    {
        [Key]
        public int ID_Medico { get; set; }

        [Required]
        [StringLength(8)]
        public string Id_Usuario { get; set; }

        public int ID_Especialidad { get; set; }

        public int ID_Contrato { get; set; }

        [StringLength(200)]
        public string? Horario_Atencion { get; set; }

        [StringLength(30)]
        public string? Telefono_Consulta { get; set; }

        // ================================
        // PROPIEDADES DE NAVEGACIÓN
        // ================================
        [ForeignKey("Id_Usuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("ID_Especialidad")]
        public virtual Especialidad Especialidad { get; set; }

        [ForeignKey("ID_Contrato")]
        public virtual TipoContrato Contrato { get; set; }
    }
}