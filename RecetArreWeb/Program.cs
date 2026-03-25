using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RecetArreWeb;
using RecetArreWeb.Auth;
using RecetArreWeb.Handlers;
using RecetArreWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Configurar HttpClient con handler de Autorizacion
//TODO Descomentar
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddScoped<HttpClient>(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7019/")
    };
});

//registrar los servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
// Registrar servicio de recetas
builder.Services.AddScoped<IRecetaService, RecetaService>();
// Registrar servicio de comentarios e ingredientes
builder.Services.AddScoped<IComentarioService, ComentarioService>();
// Ingrediente service - agregar implementación si existe
// builder.Services.AddScoped<IIngredienteService, IngredienteService>();
// TODO: Todos los demás servicios ejemplo ICategoriaService, IRecetaService, etc

//Configurar autenticación
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();


await builder.Build().RunAsync();