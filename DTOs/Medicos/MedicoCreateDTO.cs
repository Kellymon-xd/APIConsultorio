namespace ApiConsultorio.DTOs.Medicos
{
    public class MedicoCreateDTO
    {
        // Datos de usuario
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public string Contrasena { get; set; }

        // Datos de médico
        public int ID_Especialidad { get; set; }
        public int ID_Contrato { get; set; }
        public string? Horario_Atencion { get; set; }
        public string? Telefono_Consulta { get; set; }
    }
}
