namespace RecetArreWeb.DTOs
{
    public class TiempoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }

    public class TiempoCreacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }
}

