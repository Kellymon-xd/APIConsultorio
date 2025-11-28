using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ROL")]
    public class Rol
    {
        [Key]
        public byte Id_Rol { get; set; }
        public string Descripcion_Rol { get; set; }
    }
}