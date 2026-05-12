namespace RecetArreWeb.DTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace RecetArreAPI2.DTOs.Medallas
    {
        public class MedallaDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = default!;
            public string? Descripcion { get; set; }
            public int Puntos { get; set; }
        }

        public class MedallaCreacionDto
        {
            [Required]
            [StringLength(100, MinimumLength = 2)]
            public string Nombre { get; set; } = default!;

            [StringLength(500)]
            public string? Descripcion { get; set; }

            [Range(1, int.MaxValue)]
            public int Puntos { get; set; }
        }

        public class MedallaModificacionDto
        {
            [Required]
            [StringLength(100, MinimumLength = 2)]
            public string Nombre { get; set; } = default!;

            [StringLength(500)]
            public string? Descripcion { get; set; }

            [Range(1, int.MaxValue)]
            public int Puntos { get; set; }
        }

        public class UsuarioMedallaDto
        {
            public string UsuarioId { get; set; } = default!;
            public int MedallaId { get; set; }
            public DateTime FechaObtencion { get; set; }
            public MedallaDto Medalla { get; set; } = default!;
        }

        public class ProgresoMedallasDto
        {
            public int TotalRecetasCreadas { get; set; }
            public List<MedallaDto> MedallasObtenidas { get; set; } = new();
            public List<MedallaDto> MedallasDisponibles { get; set; } = new();
        }
    }

}
