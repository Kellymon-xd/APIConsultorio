using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiConsultorio.Models
{
    [Table("ANTECEDENTES_MEDICOS")]
    public class AntecedenteMedico
    {
        [Key]
        public int ID_Antecedente { get; set; }

        public int ID_Paciente { get; set; }

        [StringLength(200)]
        public string? Alergias { get; set; }

        [StringLength(200)]
        public string? Enfermedades_Cronicas { get; set; }

        [StringLength(300)]
        public string? Observaciones_Generales { get; set; }

        public DateTime Fecha_Registro { get; set; }
    }
}