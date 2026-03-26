using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RecetArreAPI2.DTOs.Identity;
using RecetArreAPI2.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecetArreAPI2.Controllers
{
    [ApiController]
    //www.localhost.com/api/cuentas
    [Route("api/[controller]")]
    public class CuentasController : Controller
    {
        /// <summary>
        /// Para dar de alta un usuario
        /// </summary>
        private readonly UserManager<ApplicationUser> userManager;
        // para ir a appsettings .json y recuperar llave JWT
        private readonly IConfiguration configuration;
        //
        private readonly SignInManager<ApplicationUser> signInManager;

        public CuentasController(UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        //Dar de alta un usuario
        //localhost.comm/api/cuentas/registrar
        [HttpPost("registrar")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new ApplicationUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            return BadRequest(resultado.Errors);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            // Primero obtener el usuario
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            var usuarioId = usuario.Id;

            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
                new Claim(ClaimTypes.Email, credencialesUsuario.Email),
                new Claim(ClaimTypes.NameIdentifier, usuarioId),
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId)
            };

            // Obtener roles del usuario
            var roles = await userManager.GetRolesAsync(usuario);
            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]!));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(30);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion,
                UsuarioId = usuarioId,
            };
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = emailClaim!.Value
            };
            return await ConstruirToken(credencialesUsuario);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Login incorrecto");
            }
        }
    }
}
