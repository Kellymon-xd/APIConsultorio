public class CrearCitaDTO
{
    public int ID_Paciente { get; set; }
    public int ID_Medico { get; set; }
    public DateTime Fecha_Cita { get; set; }
    public TimeSpan Hora_Cita { get; set; }
    public int ID_Estado_Cita { get; set; }
}