using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetArreAPI2.Context;
using RecetArreAPI2.DTOs.Recetas;
using RecetArreAPI2.DTOs.Tiempos;
using RecetArreAPI2.Models;

namespace RecetArreAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecetasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public RecetasController(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: api/recetas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecetaDto>>> GetRecetas()
        {
            var recetas = await context.Recetas
                .Include(r => r.Ing_Recs).ThenInclude(ir => ir.Ingrediente)
                .Include(r => r.Rec_Tiems).ThenInclude(rt => rt.Tiempo)
                .OrderByDescending(r => r.CreadoUtc)
                .ToListAsync();

            var result = recetas.Select(r => new RecetaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Instrucciones = r.Instrucciones,
                CreadoUtc = r.CreadoUtc,
                Ingredientes = r.Ing_Recs?.Select(ir => mapper.Map<RecetArreAPI2.DTOs.Ingredientes.IngredientesDto>(ir.Ingrediente)).ToList()
            }).ToList();

            return Ok(result);
        }

        //filtrar por categorías
        [HttpGet("filtrar/categorias")]
        public async Task<ActionResult<IEnumerable<RecetaDto>>> FiltrarPorCategorias([FromQuery] List<int> categoriaId)
        {
            if (categoriaId == null || !categoriaId.Any())
            {
                return BadRequest(new { mensaje = "No llegan los IDs Master" });
            }
            var recetas = await context.Recetas
                .Include(r => r.Cat_Recs).ThenInclude(cr => cr.Categoria)
                .Include(r => r.Ing_Recs).ThenInclude(ir => ir.Ingrediente)
                .Include(r => r.Rec_Tiems).ThenInclude(rt => rt.Tiempo)
                .Where(r => r.Cat_Recs.Any(cr => categoriaId.Contains(cr.CategoriaId)))
                .OrderByDescending(r => r.CreadoUtc)
                .ToListAsync();
            var result = recetas.Select(r => new RecetaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Instrucciones = r.Instrucciones,
                CreadoUtc = r.CreadoUtc,
                Ingredientes = r.Ing_Recs?.Select(ir => mapper.Map<RecetArreAPI2.DTOs.Ingredientes.IngredientesDto>(ir.Ingrediente)).ToList()
            }).ToList();
            return Ok(result);
        }

        // GET: api/recetas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecetaDto>> GetReceta(int id)
        {
            var receta = await context.Recetas
                .Include(r => r.Ing_Recs).ThenInclude(ir => ir.Ingrediente)
                .Include(r => r.Rec_Tiems).ThenInclude(rt => rt.Tiempo)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return NotFound(new { mensaje = "Receta no encontrada" });
            }

            var dto = new RecetaDto
            {
                Id = receta.Id,
                Nombre = receta.Nombre,
                Instrucciones = receta.Instrucciones,
                CreadoUtc = receta.CreadoUtc,
                Ingredientes = receta.Ing_Recs?.Select(ir => mapper.Map<RecetArreAPI2.DTOs.Ingredientes.IngredientesDto>(ir.Ingrediente)).ToList()
            };

            return Ok(dto);
        }

        // POST: api/recetas
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RecetaDto>> CreateReceta(RecetaCreacionDto recetaCreacionDto)
        {
            var usuarioId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized(new { mensaje = "Usuario no autenticado" });
            }

            var receta = new Receta
            {
                Nombre = recetaCreacionDto.Nombre,
                Instrucciones = recetaCreacionDto.Instrucciones,
                CreadoUtc = DateTime.UtcNow,
                CreadoPorUsuarioId = usuarioId
            };

            context.Recetas.Add(receta);
            await context.SaveChangesAsync();

            // Asociar ingredientes si se proporcionan
            if (recetaCreacionDto.IngredienteIds != null && recetaCreacionDto.IngredienteIds.Any())
            {
                foreach (var ingId in recetaCreacionDto.IngredienteIds.Distinct())
                {
                    // Opcional: validar existencia del ingrediente
                    var existeIng = await context.Ingredientes.AnyAsync(i => i.Id == ingId);
                    if (!existeIng) continue;

                    context.Ing_Recs.Add(new Ing_Rec { RecetaId = receta.Id, IngredienteId = ingId });
                }
            }

            // Asociar tiempos si se proporcionan
            if (recetaCreacionDto.TiempoIds != null && recetaCreacionDto.TiempoIds.Any())
            {
                foreach (var tiemId in recetaCreacionDto.TiempoIds.Distinct())
                {
                    var existeT = await context.Tiempos.AnyAsync(t => t.Id == tiemId);
                    if (!existeT) continue;

                    context.Rec_Tiems.Add(new Rec_Tiem { RecetaId = receta.Id, TiempoId = tiemId });
                }
            }

            await context.SaveChangesAsync();

            var dto = mapper.Map<RecetaDto>(receta);
            return CreatedAtAction(nameof(GetReceta), new { id = receta.Id }, dto);
        }

        // PUT: api/recetas/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateReceta(int id, RecetaModificacionDto recetaModificacionDto)
        {
            var receta = await context.Recetas
                .Include(r => r.Ing_Recs)
                .Include(r => r.Rec_Tiems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return NotFound(new { mensaje = "Receta no encontrada" });
            }

            // Actualizar campos básicos
            receta.Nombre = recetaModificacionDto.Nombre;
            receta.Instrucciones = recetaModificacionDto.Instrucciones;

            // Actualizar ingredientes (cascada lógica): eliminar los que ya no están, añadir los nuevos
            var nuevosIngIds = recetaModificacionDto.IngredienteIds?.Distinct().ToList() ?? new List<int>();
            var actualesIngIds = receta.Ing_Recs.Select(ir => ir.IngredienteId).ToList();

            var toRemoveIng = receta.Ing_Recs.Where(ir => !nuevosIngIds.Contains(ir.IngredienteId)).ToList();
            var toAddIng = nuevosIngIds.Where(idIng => !actualesIngIds.Contains(idIng)).ToList();

            if (toRemoveIng.Any()) context.Ing_Recs.RemoveRange(toRemoveIng);
            foreach (var addId in toAddIng)
            {
                var existe = await context.Ingredientes.AnyAsync(i => i.Id == addId);
                if (!existe) continue;
                context.Ing_Recs.Add(new Ing_Rec { RecetaId = receta.Id, IngredienteId = addId });
            }

            // Actualizar tiempos
            var nuevosTiempoIds = recetaModificacionDto.TiempoIds?.Distinct().ToList() ?? new List<int>();
            var actualesTiempoIds = receta.Rec_Tiems.Select(rt => rt.TiempoId).ToList();

            var toRemoveT = receta.Rec_Tiems.Where(rt => !nuevosTiempoIds.Contains(rt.TiempoId)).ToList();
            var toAddT = nuevosTiempoIds.Where(tid => !actualesTiempoIds.Contains(tid)).ToList();

            if (toRemoveT.Any()) context.Rec_Tiems.RemoveRange(toRemoveT);
            foreach (var addT in toAddT)
            {
                var existe = await context.Tiempos.AnyAsync(t => t.Id == addT);
                if (!existe) continue;
                context.Rec_Tiems.Add(new Rec_Tiem { RecetaId = receta.Id, TiempoId = addT });
            }

            context.Recetas.Update(receta);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Receta actualizada exitosamente", data = mapper.Map<RecetaDto>(receta) });
        }

        // DELETE: api/recetas/{id}
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteReceta(int id)
        {
            var receta = await context.Recetas.FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return NotFound(new { mensaje = "Receta no encontrada" });
            }

            // Al eliminar la receta, las relaciones en Ing_Rec y Rec_Tiem se eliminarán en cascada
            context.Recetas.Remove(receta);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Receta eliminada exitosamente" });
        }
    }
}
