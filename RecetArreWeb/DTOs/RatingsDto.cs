using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    public class RatingDto
    {
        public int Id { get; set; }
        public int Calificacion { get; set; }
        public DateTime CalificadoUtc { get; set; }

        public string? CalificadoPorUsuarioId { get; set; }
        public string? CalificadoPorUsuarioNombre { get; set; }

        public int RecetaId { get; set; }
    }

    public class RatingCreacionDto
    {
        [Range(1, 5)]
        public int Calificacion { get; set; }

        public int RecetaId { get; set; }
    }

    public class RatingModificacionDto
    {
        [Range(1, 5)]
        public int Calificacion { get; set; }
    }
}
