namespace RecetArreWeb.DTOs
{
    using RecetArreWeb.DTOs;
    using RecetArreWeb.DTOs.RecetArreAPI2.DTOs.Medallas;
    using System;
    using System.Collections.Generic;

    public class RecetaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Instrucciones { get; set; }
        public DateTime CreadoUtc { get; set; }

        // Lista de ingredientes incluidos en la receta 
        public List<IngredientesDto>? Ingredientes { get; set; }

        // Lista de tiempos asociados a la receta 
        public List<TiempoDto>? Tiempos { get; set; }

        // Información de calificaciones
        public double? PromedioCalificaciones { get; set; }
        public int TotalCalificaciones { get; set; }
    }

    public class RecetaCreacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Instrucciones { get; set; }

        // IDs de ingredientes que formarán parte de la receta
        public List<int>? IngredienteIds { get; set; }

        // IDs de tiempos asociados (opcional)
        public List<int>? TiempoIds { get; set; }
    }

    public class RecetaModificacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Instrucciones { get; set; }
        public List<int>? IngredienteIds { get; set; }
        public List<int>? TiempoIds { get; set; }
    }

    /// <summary>
    /// // medallas
    /// </summary>
    public class RecetaCreacionRespuestaDto
    {
        public RecetaDto Receta { get; set; } = default!;
        public List<MedallaDto> MedallasOtorgadas { get; set; } = new();
    }

}
