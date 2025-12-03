namespace ApiConsultorio.DTOs
{
    public class AtencionMedicaDTO
    {
        public int Id_Atencion { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
    }
}