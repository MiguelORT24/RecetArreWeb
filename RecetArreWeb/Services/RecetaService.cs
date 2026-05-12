using RecetArreWeb.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace RecetArreWeb.Services
{
    public interface IRecetaService
    {
        string UltimoError { get; }
        Task<List<RecetaDto>> ObtenerTodas();
        Task<RecetaDto?> ObtenerPorId(int id);
        Task<RecetaCreacionRespuestaDto?> Crear(RecetaCreacionDto dto);
        Task<bool> Actualizar(int id, RecetaModificacionDto dto);
        Task<bool> Eliminar(int id);
        Task<List<RecetaRankingDto>> ObtenerRanking(int cantidad = 3);
    }

    public class RecetaService : IRecetaService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;
        private readonly IRatingService? ratingService;
        private const string endpoint = "api/recetas";
        public string UltimoError { get; private set; } = string.Empty;

        public RecetaService(HttpClient httpClient, ITokenService tokenService, IRatingService? ratingService = null)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
            this.ratingService = ratingService;
        }

        public async Task<List<RecetaDto>> ObtenerTodas()
        {
            UltimoError = string.Empty;
            try
            {
                var r = await httpClient.GetFromJsonAsync<List<RecetaDto>>(endpoint);
                return r ?? new List<RecetaDto>();
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudieron obtener las recetas.";
                Console.WriteLine($"Error al obtener recetas: {ex.Message}");
                return new List<RecetaDto>();
            }
        }

        public async Task<RecetaDto?> ObtenerPorId(int id)
        {
            UltimoError = string.Empty;
            try
            {
                return await httpClient.GetFromJsonAsync<RecetaDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo obtener la receta.";
                Console.WriteLine($"Error al obtener receta {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<RecetaCreacionRespuestaDto?> Crear(RecetaCreacionDto dto)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para crear recetas.";
                return null;
            }

            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RecetaCreacionRespuestaDto>();
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo crear la receta.");
                return null;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al crear la receta.";
                Console.WriteLine($"Error al crear receta: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, RecetaModificacionDto dto)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para actualizar recetas.";
                return false;
            }

            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo actualizar la receta.");
                return false;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al actualizar la receta.";
                Console.WriteLine($"Error al actualizar receta {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para eliminar recetas.";
                return false;
            }

            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo eliminar la receta.");
                return false;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al eliminar la receta.";
                Console.WriteLine($"Error al eliminar receta {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RecetaRankingDto>> ObtenerRanking(int cantidad = 3)
        {
            UltimoError = string.Empty;
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
                UltimoError = "No se pudo obtener el ranking.";
                Console.WriteLine($"Error al obtener ranking: {ex.Message}");
                return new List<RecetaRankingDto>();
            }
        }

        private static async Task<string> ConstruirMensajeError(HttpResponseMessage response, string mensajeDefault)
        {
            var detalle = await IntentarLeerMensaje(response);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "No autorizado. Inicia sesión nuevamente.",
                System.Net.HttpStatusCode.Forbidden => "No tienes permisos para realizar esta acción.",
                System.Net.HttpStatusCode.NotFound => detalle ?? "No se encontró el recurso solicitado.",
                System.Net.HttpStatusCode.BadRequest => detalle ?? "Solicitud inválida.",
                _ => detalle ?? mensajeDefault
            };
        }

        private static async Task<string?> IntentarLeerMensaje(HttpResponseMessage response)
        {
            try
            {
                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return null;
                }

                using var document = JsonDocument.Parse(raw);
                if (document.RootElement.TryGetProperty("mensaje", out var mensaje))
                {
                    return mensaje.GetString();
                }

                return raw;
            }
            catch
            {
                return null;
            }
        }
    }
}
