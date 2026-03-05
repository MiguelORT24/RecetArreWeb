namespace RecetArreWeb.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        public DateTime CreadoUtc { get; set; }
    }

    public class CategoriaCreacionDto
    {   
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }

    public class CategoriaModificacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }
}
