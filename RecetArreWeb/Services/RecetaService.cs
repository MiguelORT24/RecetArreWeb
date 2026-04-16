using RecetArreWeb.DTOs;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    public interface IRecetaService
    {
        Task<List<RecetaDto>> ObtenerTodas();
        Task<RecetaDto?> ObtenerPorId(int id);
        Task<RecetaDto?> Crear(RecetaCreacionDto dto);
        Task<bool> Actualizar(int id, RecetaModificacionDto dto);
        Task<bool> Eliminar(int id);
        Task<List<RecetaRankingDto>> ObtenerRanking(int cantidad = 3);
    }

    public class RecetaService : IRecetaService
    {
        private readonly HttpClient httpClient;
        private readonly IRatingService? ratingService;
        private const string endpoint = "api/recetas";

        public RecetaService(HttpClient httpClient, IRatingService? ratingService = null)
        {
            this.httpClient = httpClient;
            this.ratingService = ratingService;
        }

        public async Task<List<RecetaDto>> ObtenerTodas()
        {
            try
            {
                var r = await httpClient.GetFromJsonAsync<List<RecetaDto>>(endpoint);
                return r ?? new List<RecetaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener recetas: {ex.Message}");
                return new List<RecetaDto>();
            }
        }

        public async Task<RecetaDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<RecetaDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener receta {id}: {ex.Message}");
                return null;
            }
        }
        public async Task<RecetaDto?> Crear(RecetaCreacionDto dto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RecetaDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear receta: {err}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear receta: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, RecetaModificacionDto dto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar receta {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar receta {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RecetaRankingDto>> ObtenerRanking(int cantidad = 3)
        {
            try
            {
                // Obtener todas las recetas
                var recetas = await ObtenerTodas();

                if (recetas == null || recetas.Count == 0)
                    return new List<RecetaRankingDto>();

                var ranking = new List<RecetaRankingDto>();

                foreach (var receta in recetas)
                {
                    double promedioCalificacion = 0;
                    int totalCalificaciones = 0;

                    // Si tenemos acceso a RatingService, calcular el promedio real
                    if (ratingService != null)
                    {
                        try
                        {
                            var todasCalificaciones = await ratingService.ObtenerTodosRatingsReceta(receta.Id);
                            if (todasCalificaciones != null && todasCalificaciones.Count > 0)
                            {
                                totalCalificaciones = todasCalificaciones.Count;
                                promedioCalificacion = todasCalificaciones.Average(r => r.Calificacion);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al obtener calificaciones para receta {receta.Id}: {ex.Message}");
                        }
                    }

                    ranking.Add(new RecetaRankingDto
                    {
                        Id = receta.Id,
                        Nombre = receta.Nombre,
                        PromedioCalificacion = promedioCalificacion,
                        TotalCalificaciones = totalCalificaciones
                    });
                }

                // Ordenar por promedio descendente y tomar los top N
                var topRecetas = ranking
                    .OrderByDescending(r => r.PromedioCalificacion)
                    .ThenByDescending(r => r.TotalCalificaciones)
                    .Take(cantidad)
                    .Select((r, index) => new RecetaRankingDto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        PromedioCalificacion = r.PromedioCalificacion,
                        TotalCalificaciones = r.TotalCalificaciones,
                        Posicion = index + 1
                    })
                    .ToList();

                return topRecetas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ranking: {ex.Message}");
                return new List<RecetaRankingDto>();
            }
        }
    }
}
