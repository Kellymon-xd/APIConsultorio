namespace ApiConsultorio.DTOs
{
    public class CrearAtencionDTO
    {
        public int ID_Cita { get; set; }
        public string Motivo_Consulta { get; set; }
        public string? Diagnostico { get; set; }
        public string? Observaciones { get; set; }
        public DateTime Fecha { get; set; }
    }
}