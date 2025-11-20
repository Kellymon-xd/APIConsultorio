public class MostrarUsuarioDTO
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Cedula { get; set; }
    public string? Telefono { get; set; }
    public byte IdRol { get; set; }   // 1=A, 2=M, 3=S
}
