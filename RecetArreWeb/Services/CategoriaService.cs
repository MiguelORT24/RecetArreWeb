using Microsoft.IdentityModel.Configuration;
using RecetArreWeb.DTOs;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RecetArreWeb.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> ObtenerTodasCategorias();
        Task<List<CategoriaDto>> ObtenerTodas();
        Task<CategoriaDto?> ObtenerCategoriaPorId(int id);
        Task<CategoriaDto?> CrearCategoria(CategoriaCreacionDto categoria);
        Task<bool> ActualizarCategoria(int id, CategoriaModificacionDto categoriaModificacionDto);
        Task<bool> EliminarCategoria(int id);
    }

    public class CategoriaService : ICategoriaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/categorias";

        public CategoriaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<CategoriaDto>> ObtenerTodasCategorias()
        {
            try
            {
                var categorias = await httpClient.GetFromJsonAsync<List<CategoriaDto>>(endpoint);
                return categorias ?? new List<CategoriaDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener categorías: {ex.Message}");
                return new List<CategoriaDto>();
            }
        }

        public async Task<CategoriaDto?> ObtenerCategoriaPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<CategoriaDto>($"{endpoint}/{id}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la categoría con ID {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<CategoriaDto?> CrearCategoria(CategoriaCreacionDto categoriaDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, categoriaDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CategoriaDto>();
                }
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear categorías: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear categorías: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ActualizarCategoria(int id, CategoriaModificacionDto categoriaDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", categoriaDto);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al actualizar la categoría  {id}: {error}");
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar la categoría {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarCategoria(int id)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al eliminar la categoría {id}: {error}");
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar la categoría {id}: {ex.Message}");
                return false;
            }
        }

        // Compatibility wrapper used by pages
        public Task<List<CategoriaDto>> ObtenerTodas()
        {
            return ObtenerTodasCategorias();
        }
    }
}
