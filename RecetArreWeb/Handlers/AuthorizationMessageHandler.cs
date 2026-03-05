using RecetArreWeb.Services;
using System.Net.Http.Headers;

namespace RecetArreWeb.Handlers
{
    //DelegatingHandler: clase base que permite interceptar las solicitudes HTTP salientes y
    //modificar el mensaje antes de que se envíe al servidor.
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ITokenService tokenService;
        public AuthorizationMessageHandler(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await tokenService.ObtenerToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }


    }
}
