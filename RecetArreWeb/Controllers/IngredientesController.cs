using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetArreAPI2.Context;
using RecetArreAPI2.DTOs.Ingredientes;
using RecetArreAPI2.Models;

namespace RecetArreAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public IngredientesController(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: api/ingredientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientesDto>>> GetIngredientes()
        {
            var ingredientes = await context.Ingredientes
                .OrderByDescending(i => i.Nombre)
                .ToListAsync();

            return Ok(mapper.Map<List<IngredientesDto>>(ingredientes));
        }

        // GET: api/ingredientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientesDto>> GetIngrediente(int id)
        {
            var ingrediente = await context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id);

            if (ingrediente == null)
            {
                return NotFound(new { mensaje = "Ingrediente no encontrado" });
            }

            return Ok(mapper.Map<IngredientesDto>(ingrediente));
        }

        // POST: api/ingredientes
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IngredientesDto>> CreateIngrediente(IngredienteCreacionDto ingredienteCreacionDto)
        {
            var existe = await context.Ingredientes
                .AnyAsync(i => i.Nombre.ToLower() == ingredienteCreacionDto.Nombre.ToLower());

            if (existe)
            {
                return BadRequest(new { mensaje = "Ya existe un ingrediente con ese nombre" });
            }

            var usuarioId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized(new { mensaje = "Usuario no autenticado" });
            }

            var ingrediente = mapper.Map<Ingrediente>(ingredienteCreacionDto);
            //Podemos seguir cambiando valores a ese mapa después de mapearlo, no es necesario que todo venga del DTO
            ingrediente.CreadoUtc = DateTime.UtcNow;
            ingrediente.CreadoPorUsuarioId = usuarioId;

            context.Ingredientes.Add(ingrediente);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIngrediente), new { id = ingrediente.Id }, mapper.Map<IngredientesDto>(ingrediente));
        }

        // PUT: api/ingredientes/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateIngrediente(int id, IngredienteModificacionDto ingredienteModificacionDto)
        {
            var ingrediente = await context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id);

            if (ingrediente == null)
            {
                return NotFound(new { mensaje = "Ingrediente no encontrado" });
            }

            //Verificamos si el nombre se está modificando, si es así,
            //verificamos que no exista otro ingrediente con ese nombre
            if (!ingrediente.Nombre.Equals(ingredienteModificacionDto.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                var existe = await context.Ingredientes
                    .AnyAsync(i => i.Nombre.ToLower() == ingredienteModificacionDto.Nombre.ToLower() && i.Id != id);

                if (existe)
                {
                    return BadRequest(new { mensaje = "Ya existe un ingrediente con ese nombre" });
                }
            }

            mapper.Map(ingredienteModificacionDto, ingrediente);
            context.Ingredientes.Update(ingrediente);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Ingrediente actualizado exitosamente", data = mapper.Map<IngredientesDto>(ingrediente) });
        }

        // DELETE: api/ingredientes/{id}
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteIngrediente(int id)
        {
            var ingrediente = await context.Ingredientes.FirstOrDefaultAsync(i => i.Id == id);

            if (ingrediente == null)
            {
                return NotFound(new { mensaje = "Ingrediente no encontrado" });
            }

            context.Ingredientes.Remove(ingrediente);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Ingrediente eliminado exitosamente" });
        }
    }
}
