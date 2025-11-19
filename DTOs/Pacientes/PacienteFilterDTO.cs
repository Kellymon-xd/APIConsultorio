namespace Proyecto.DTOs.Pacientes
{
    public class PacienteFilterDto
    {
        public string? Nombre { get; set; }
        public string? Cedula { get; set; }
        public bool? Activo { get; set; }
    }
}
