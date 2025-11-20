public class AntecedentesDetalleDTO
{
    public int ID_Antecedente { get; set; }
    public int ID_Paciente { get; set; }
    public string NombrePaciente { get; set; }
    public string? Alergias { get; set; }
    public string? Enfermedades_Cronicas { get; set; }
    public string? Observaciones_Generales { get; set; }
    public DateTime Fecha_Registro { get; set; }
}