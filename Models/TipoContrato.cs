using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("TIPO_CONTRATO")]
    public class TipoContrato
    {
        [Key]
        public int ID_Contrato { get; set; }

        [StringLength(30)]
        public string Descripcion { get; set; }
    }
}
