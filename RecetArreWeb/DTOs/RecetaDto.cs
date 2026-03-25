namespace RecetArreWeb.DTOs
{
    using RecetArreWeb.DTOs;
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
}

