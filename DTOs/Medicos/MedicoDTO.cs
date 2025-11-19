namespace ApiConsultorio.DTOs.Medicos
{
    public class MedicoDetailDTO
    {
        public int ID_Medico { get; set; }

        // Datos del usuario
        public string Id_Usuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }

        // Datos del médico
        public int ID_Especialidad { get; set; }
        public string Nombre_Especialidad { get; set; }
        public int ID_Contrato { get; set; }
        public string Tipo_Contrato { get; set; }
        public string? Horario_Atencion { get; set; }
        public string? Telefono_Consulta { get; set; }
        public bool Activo { get; set; }
    }
}
