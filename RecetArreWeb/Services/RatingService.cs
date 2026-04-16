using RecetArreWeb.DTOs;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    public interface IRatingService
    {
        Task<RatingDto?> ObtenerRatingReceta(int recetaId);
        Task<List<RatingDto>> ObtenerTodosRatingsReceta(int recetaId);
        Task<RatingDto?> CrearOActualizarRating(RatingCreacionDto dto);
        Task<bool> EliminarRating(int id);
    }

    public class RatingService : IRatingService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/ratings";

        public RatingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<RatingDto?> ObtenerRatingReceta(int recetaId)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<RatingDto>($"{endpoint}/receta/{recetaId}/usuario");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener rating de receta {recetaId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RatingDto>> ObtenerTodosRatingsReceta(int recetaId)
        {
            try
            {
                var ratings = await httpClient.GetFromJsonAsync<List<RatingDto>>($"{endpoint}/receta/{recetaId}/todos");
                return ratings ?? new List<RatingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ratings de receta {recetaId}: {ex.Message}");
                return new List<RatingDto>();
            }
        }

        public async Task<RatingDto?> CrearOActualizarRating(RatingCreacionDto dto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RatingDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear/actualizar rating: {err}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear/actualizar rating: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EliminarRating(int id)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar rating {id}: {ex.Message}");
                return false;
            }
        }
    }
}
