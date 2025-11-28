public class MostrarUsuarioDetalleDTO
{
    public string Id_Usuario { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string Cedula { get; set; }
    public byte IdRol { get; set; }
    public string? Telefono { get; set; }
    public DateTime Fecha_Registro { get; set; }
}
