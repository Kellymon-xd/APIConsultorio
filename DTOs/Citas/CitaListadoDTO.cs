public class CitaListadoDTO
{
    public int ID_Cita { get; set; }
    public string NombrePaciente { get; set; }
    public string NombreMedico { get; set; }
    public string Especialidad { get; set; }
    public DateTime Fecha_Cita { get; set; }
    public TimeSpan Hora_Cita { get; set; }
    public string Estado { get; set; }
}