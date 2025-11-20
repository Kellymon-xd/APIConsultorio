public class AtencionDetalleDTO
{
    public int ID_Atencion { get; set; }
    public DateTime Fecha_Atencion { get; set; }
    public string Motivo_Consulta { get; set; }
    public string? Diagnostico { get; set; }
    public string? Observaciones { get; set; }
    public int ID_Cita { get; set; }
    public string NombrePaciente { get; set; }
    public string NombreMedico { get; set; }
}