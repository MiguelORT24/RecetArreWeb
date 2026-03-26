using RecetArreWeb.DTOs;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    public interface IIngredienteService
    {
        Task<List<IngredientesDto>> ObtenerTodos();
        Task<IngredientesDto?> ObtenerPorId(int id);
        Task<IngredientesDto?> Crear(IngredienteCreacionDto dto);
        Task<bool> Actualizar(int id, IngredienteModificacionDto dto);
        Task<bool> Eliminar(int id);
    }

    public class IngredienteService : IIngredienteService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/ingredientes";

        public IngredienteService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<IngredientesDto>> ObtenerTodos()
        {
            try
            {
                var lista = await httpClient.GetFromJsonAsync<List<IngredientesDto>>(endpoint);
                return lista ?? new List<IngredientesDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingredientes: {ex.Message}");
                return new List<IngredientesDto>();
            }
        }

        public async Task<IngredientesDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<IngredientesDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingrediente {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<IngredientesDto?> Crear(IngredienteCreacionDto dto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IngredientesDto>();
                }
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear ingrediente: {err}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear ingrediente: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, IngredienteModificacionDto dto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ingrediente {id}: {ex.Message}");
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
                Console.WriteLine($"Error al eliminar ingrediente {id}: {ex.Message}");
                return false;
            }
        }
    }
}
