using RecetArreWeb.DTOs.RecetArreAPI2.DTOs.Medallas;
using System.Net.Http.Json;
using System.Text.Json;

namespace RecetArreWeb.Services
{
    public interface IMedallaService
    {
        string UltimoError { get; }
        Task<List<MedallaDto>> ObtenerTodas();
        Task<MedallaDto?> ObtenerPorId(int id);
        Task<List<UsuarioMedallaDto>> ObtenerMisMedallas();
        Task<ProgresoMedallasDto?> ObtenerProgreso();
        Task<MedallaDto?> Crear(MedallaCreacionDto dto);
        Task<bool> Actualizar(int id, MedallaModificacionDto dto);
        Task<bool> Eliminar(int id);
    }

    public class MedallaService : IMedallaService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;
        private const string endpoint = "api/medallas";

        public string UltimoError { get; private set; } = string.Empty;

        public MedallaService(HttpClient httpClient, ITokenService tokenService)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
        }

        public async Task<List<MedallaDto>> ObtenerTodas()
        {
            UltimoError = string.Empty;

            try
            {
                var data = await httpClient.GetFromJsonAsync<List<MedallaDto>>(endpoint);
                return data ?? new List<MedallaDto>();
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudieron obtener las medallas.";
                Console.WriteLine($"Error al obtener medallas: {ex.Message}");
                return new List<MedallaDto>();
            }
        }

        public async Task<MedallaDto?> ObtenerPorId(int id)
        {
            UltimoError = string.Empty;

            try
            {
                return await httpClient.GetFromJsonAsync<MedallaDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo obtener la medalla.";
                Console.WriteLine($"Error al obtener medalla {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UsuarioMedallaDto>> ObtenerMisMedallas()
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para ver tus medallas.";
                return new List<UsuarioMedallaDto>();
            }

            try
            {
                var data = await httpClient.GetFromJsonAsync<List<UsuarioMedallaDto>>($"{endpoint}/mis-medallas");
                return data ?? new List<UsuarioMedallaDto>();
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo obtener el listado de tus medallas.";
                Console.WriteLine($"Error al obtener mis medallas: {ex.Message}");
                return new List<UsuarioMedallaDto>();
            }
        }

        public async Task<ProgresoMedallasDto?> ObtenerProgreso()
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para ver tu progreso.";
                return null;
            }

            try
            {
                return await httpClient.GetFromJsonAsync<ProgresoMedallasDto>($"{endpoint}/progreso");
            }
            catch (Exception ex)
            {
                UltimoError = "No se pudo obtener el progreso de medallas.";
                Console.WriteLine($"Error al obtener progreso de medallas: {ex.Message}");
                return null;
            }
        }

        public async Task<MedallaDto?> Crear(MedallaCreacionDto dto)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para crear medallas.";
                return null;
            }

            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, dto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MedallaDto>();
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo crear la medalla.");
                return null;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al crear la medalla.";
                Console.WriteLine($"Error al crear medalla: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, MedallaModificacionDto dto)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para actualizar medallas.";
                return false;
            }

            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo actualizar la medalla.");
                return false;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al actualizar la medalla.";
                Console.WriteLine($"Error al actualizar medalla {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            UltimoError = string.Empty;

            if (!await tokenService.EstaAutenticado())
            {
                UltimoError = "Debes iniciar sesión para eliminar medallas.";
                return false;
            }

            try
            {
                var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                UltimoError = await ConstruirMensajeError(response, "No se pudo eliminar la medalla.");
                return false;
            }
            catch (Exception ex)
            {
                UltimoError = "Error de red al eliminar la medalla.";
                Console.WriteLine($"Error al eliminar medalla {id}: {ex.Message}");
                return false;
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
