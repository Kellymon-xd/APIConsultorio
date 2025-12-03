namespace ApiConsultorio.DTOs
{
    public class CitaListadoDTO
    {
        public int ID_Cita { get; set; }
        public string NombrePaciente { get; set; }
        public string NombreMedico { get; set; }
        public string Especialidad { get; set; }
        public string Fecha_Cita { get; set; }
        public string Hora_Cita { get; set; }
        public int ID_Estado_Cita { get; set; }
    }
}