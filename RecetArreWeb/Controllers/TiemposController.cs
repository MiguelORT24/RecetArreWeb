using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetArreAPI2.Context;
using RecetArreAPI2.DTOs.Tiempos;
using RecetArreAPI2.Models;

namespace RecetArreAPI2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiemposController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public TiemposController(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: api/tiempos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TiempoDto>>> GetTiempos()
        {
            var tiempos = await context.Tiempos
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return Ok(mapper.Map<List<TiempoDto>>(tiempos));
        }

        // GET: api/tiempos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TiempoDto>> GetTiempo(int id)
        {
            var tiempo = await context.Tiempos.FirstOrDefaultAsync(t => t.Id == id);

            if (tiempo == null)
            {
                return NotFound(new { mensaje = "Tiempo no encontrado" });
            }

            return Ok(mapper.Map<TiempoDto>(tiempo));
        }

        // POST: api/tiempos
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<TiempoDto>> CreateTiempo(TiempoCreacionDto tiempoCreacionDto)
        {
            var existe = await context.Tiempos.AnyAsync(t => t.Nombre.ToLower() == tiempoCreacionDto.Nombre.ToLower());
            if (existe)
            {
                return BadRequest(new { mensaje = "Ya existe un tiempo con ese nombre" });
            }

            var tiempo = mapper.Map<Tiempo>(tiempoCreacionDto);
            context.Tiempos.Add(tiempo);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTiempo), new { id = tiempo.Id }, mapper.Map<TiempoDto>(tiempo));
        }

        // PUT: api/tiempos/{id}
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateTiempo(int id, TiempoCreacionDto tiempoCreacionDto)
        {
            var tiempo = await context.Tiempos.FirstOrDefaultAsync(t => t.Id == id);
            if (tiempo == null)
            {
                return NotFound(new { mensaje = "Tiempo no encontrado" });
            }

            if (!tiempo.Nombre.Equals(tiempoCreacionDto.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                var existe = await context.Tiempos.AnyAsync(t => t.Nombre.ToLower() == tiempoCreacionDto.Nombre.ToLower() && t.Id != id);
                if (existe) return BadRequest(new { mensaje = "Ya existe un tiempo con ese nombre" });
            }

            mapper.Map(tiempoCreacionDto, tiempo);
            context.Tiempos.Update(tiempo);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Tiempo actualizado exitosamente", data = mapper.Map<TiempoDto>(tiempo) });
        }

        // DELETE: api/tiempos/{id}
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteTiempo(int id)
        {
            var tiempo = await context.Tiempos.FirstOrDefaultAsync(t => t.Id == id);
            if (tiempo == null)
            {
                return NotFound(new { mensaje = "Tiempo no encontrado" });
            }

            context.Tiempos.Remove(tiempo);
            await context.SaveChangesAsync();

            return Ok(new { mensaje = "Tiempo eliminado exitosamente" });
        }
    }
}
