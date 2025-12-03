using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ESPECIALIDADES")]
    public class Especialidad
    {
        [Key]
        public int ID_Especialidad { get; set; }

        [StringLength(50)]
        public string Nombre_Especialidad { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }
    }
}