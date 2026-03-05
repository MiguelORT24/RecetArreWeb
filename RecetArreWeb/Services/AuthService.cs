using RecetArreWeb.DTOs;
using System.Net.Http.Json;

namespace RecetArreWeb.Services
{
    public interface IAuthService
    {
        Task<RespuestaAutenticacion?> Login(CredencialesUsuario credencialesUsuario);
        Task<RespuestaAutenticacion?> Registrar(CredencialesUsuario credencialesUsuario);
        Task<RespuestaAutenticacion?> RenovarToken();
        Task Logout();
    }
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly ITokenService tokenService;

        private const string endpoint = "api/Cuentas";
        public AuthService(HttpClient httpClient, ITokenService tokenService = null)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
        }


        public async Task<RespuestaAutenticacion?> Login(CredencialesUsuario credencialesUsuario)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"{endpoint}/Login", credencialesUsuario);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta y extraer el token
                    var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();

                    if (respuesta != null)
                    {
                        await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
                        return respuesta;
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error en Login: {error}");
                }
                return null;
            }
           
            catch (Exception ex)
            {
                Console.WriteLine($"Error al hacer login: {ex.Message}");
                return null;
            }
            
        }

        public async Task Logout()
        {
            await tokenService.EliminarToken();
        }

        public async Task<RespuestaAutenticacion?> Registrar(CredencialesUsuario credenciales)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"{endpoint}/registrar", credenciales);

                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();

                    if (respuesta != null)
                    {
                        if (tokenService != null)
                        {
                            await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
                        }
                        return respuesta;
                    }
                }
                return null;
                //else
                //{
                //    var error = await response.Content.ReadAsStringAsync();
                //    Console.WriteLine($"Error en Registrar: {error}");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar usuario: {ex.Message}");
            }

            return null;
        }

        public async Task<RespuestaAutenticacion?> RenovarToken()
        {
            try
            {
                var token = await tokenService.ObtenerToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null; 
                }
                //agregar el token actual al header
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{endpoint}/RenovarToken");

                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();

                    if(response != null)
                    {
                        await tokenService.GuardarToken(respuesta!.Token, respuesta.Expiracion);
                        return respuesta; 
                    }
                }
                return null;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al renovar token: {ex.Message}");
                return null;
            }
            
        }
        
    }
}