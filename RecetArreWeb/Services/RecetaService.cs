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
    }

    public class RecetaService : IRecetaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/recetas";

        public RecetaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
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
    }
}
