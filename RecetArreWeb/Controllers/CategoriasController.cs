using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetArreAPI2.Context;
using RecetArreAPI2.DTOs.Categorias;
using RecetArreAPI2.Models;

namespace RecetArreAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public CategoriasController(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            var categorias = await context.Categorias
                .OrderByDescending(c => c.CreadoUtc)
                .ToListAsync();

            return Ok(mapper.Map<List<CategoriaDto>>(categorias));
        }

        // GET: api/categorias/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }

            return Ok(mapper.Map<CategoriaDto>(categoria));
        }

        // POST: api/categorias
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CategoriaCreacionDto categoriaCreacionDto)
        {
            // Validar que el nombre no esté duplicado
            var existe = await context.Categorias
                .AnyAsync(c => c.Nombre.ToLower() == categoriaCreacionDto.Nombre.ToLower());

            if (existe)
            {
                return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });
            }

            // Obtener el usuario autenticado
            var usuarioId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized(new { mensaje = "Usuario no autenticado" });
            }

            var categoria = mapper.Map<Categoria>(categoriaCreacionDto);
            categoria.CreadoUtc = DateTime.UtcNow;
            categoria.CreadoPorUsuarioId = usuarioId;

            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, mapper.Map<CategoriaDto>(categoria));
        }

        // PUT: api/categorias/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCategoria(int id, CategoriaModificacionDto categoriaModificacionDto)
        {
            var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }

            // Validar que el nombre no esté duplicado (si cambió)
            if (!categoria.Nombre.Equals(categoriaModificacionDto.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                var existe = await context.Categorias
                    .AnyAsync(c => c.Nombre.ToLower() == categoriaModificacionDto.Nombre.ToLower() && c.Id != id);

                if (existe)
                {
                    return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });
                }
            }

            mapper.Map(categoriaModificacionDto, categoria);
            context.Categorias.Update(categoria);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría actualizada exitosamente", data = mapper.Map<CategoriaDto>(categoria) });
        }

        // DELETE: api/categorias/{id}
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }

            context.Categorias.Remove(categoria);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Categoría eliminada exitosamente" });
        }
    }
}
