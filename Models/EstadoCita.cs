using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ESTADO_CITA")]
    public class EstadoCita
    {
        [Key]
        public int ID_Estado_Cita { get; set; }

        [StringLength(30)]
        public string Descripcion { get; set; }
    }
}