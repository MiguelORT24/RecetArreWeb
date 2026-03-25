
using System;

namespace RecetArreWeb.DTOs

{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = default!;
        public DateTime CreadoUtc { get; set; }

        // Referencia mínima al autor
        public string? CreadoPorUsuarioId { get; set; }
        public string? CreadoPorUsuarioNombre { get; set; }

        // Referencia a receta
        public int RecetaId { get; set; }
    }

    public class ComentarioCreacionDto
    {
        public string Contenido { get; set; } = default!;
        public int RecetaId { get; set; }
    }

    public class ComentarioModificacionDto
    {
        public string Contenido { get; set; } = default!;
    }
}

