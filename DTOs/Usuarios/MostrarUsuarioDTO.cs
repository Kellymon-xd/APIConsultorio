namespace ApiConsultorio.DTOs
{
    public class MostrarUsuarioDTO
    {
        public string Id_Usuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Cedula { get; set; }
        public byte Id_Rol { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}