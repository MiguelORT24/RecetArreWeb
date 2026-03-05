using Microsoft.JSInterop;

namespace RecetArreWeb.Services
{
    public interface ITokenService
    {
        Task GuardarToken(string Token, DateTime expiracion);
        Task<string> ObtenerToken();
        Task<DateTime?> ObtenerExpiracion();
        Task<bool> EstaAutenticado();
        Task EliminarToken();

        
    }

    public class TokenService : ITokenService
    {
        private readonly IJSRuntime jsRuntime;
        private const string TOKEN_KEY = "authtoken";
        private const string EXPIRACION_KEY = "tokenExpiracion";

        public TokenService(IJSRuntime jSRuntime)
        {
            this.jsRuntime = jSRuntime;
        }

        Task ITokenService.EliminarToken()
        {
            throw new NotImplementedException();
        }

        public async Task EliminarToken()
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", EXPIRACION_KEY);

        }

        public async Task<bool> EstaAutenticado()
        {
            var token = await ObtenerToken();
            return !string.IsNullOrEmpty(token);
        }

        async Task ITokenService.GuardarToken(string token, DateTime expiracion)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", EXPIRACION_KEY, expiracion.ToString("o"));
            //Formato ISO 8601 para asegurar que se guarde correctamente la fecha y hora

        }

        public async Task<DateTime?> ObtenerExpiracion()
        {
            try
            {
                var expiraciinStr = await jsRuntime.InvokeAsync<string>("localStorage.getItem", EXPIRACION_KEY);

                if (string.IsNullOrEmpty(expiraciinStr))
                    return null;

                if (DateTime.TryParse(expiraciinStr, out var expiracion))
                    return expiracion;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la expiración del token: {ex.Message}");
            }


            return DateTime.MinValue; // Si no se pudo parsear, devolver una fecha mínima
        }

        public async Task<string> ObtenerToken()
        {
            try
            {
                //1. Leer el token de LocalStorage
                var token = await jsRuntime.InvokeAsync<string?>("local.Storage.getItem", TOKEN_KEY);
                //2. Si no hay toquek, devolver null
                if (string.IsNullOrEmpty(token))
                    return null;

                //Verificar si el token expiró
                var expiracion = await ObtenerExpiracion();
                if (expiracion.HasValue && expiracion.Value < DateTime.UtcNow)
                {
                    //Token expirado, eliminarlo y devolver null
                    await EliminarToken();
                    return null;
                }
                return token;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el token: {ex.Message}");
                return null;
            }

            return await jsRuntime.InvokeAsync<string>("local.Storage.getItem", TOKEN_KEY);
        }
    }
}
