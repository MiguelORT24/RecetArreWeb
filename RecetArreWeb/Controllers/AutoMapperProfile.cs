using AutoMapper;
using RecetArreAPI2.DTOs;
using RecetArreAPI2.DTOs.Categorias;
using RecetArreAPI2.DTOs.Ingredientes;
using RecetArreAPI2.DTOs.Recetas;
using RecetArreAPI2.DTOs.Tiempos;
using RecetArreAPI2.DTOs.Comentarios;
using RecetArreAPI2.Models;
using System.Linq;

namespace RecetArreAPI2.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ApplicationUser <-> ApplicationUserDto
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();

            // Categoria mappings
            CreateMap<Categoria, CategoriaDto>();
            CreateMap<CategoriaCreacionDto, Categoria>();
            CreateMap<CategoriaModificacionDto, Categoria>();

            // Ingrediente
            CreateMap<Ingrediente, IngredientesDto>();
            CreateMap<IngredienteCreacionDto, Ingrediente>();
            CreateMap<IngredienteModificacionDto, Ingrediente>();

            // Tiempo
            CreateMap<Tiempo, TiempoDto>();
            CreateMap<TiempoCreacionDto, Tiempo>();

            // Receta
            CreateMap<Receta, RecetaDto>()
                .ForMember(dest => dest.Ingredientes, opt => opt.MapFrom(src => src.Ing_Recs.Select(ir => ir.Ingrediente)))
                .ForMember(dest => dest.Tiempos, opt => opt.MapFrom(src => src.Rec_Tiems.Select(rt => rt.Tiempo)));

            CreateMap<RecetaCreacionDto, Receta>()
                .ForMember(dest => dest.Ing_Recs, opt => opt.Ignore())
                .ForMember(dest => dest.Rec_Tiems, opt => opt.Ignore());

            CreateMap<RecetaModificacionDto, Receta>()
                .ForMember(dest => dest.Ing_Recs, opt => opt.Ignore())
                .ForMember(dest => dest.Rec_Tiems, opt => opt.Ignore());

            // Comentario
            CreateMap<Comentario, ComentarioDto>()
                .ForMember(dest => dest.CreadoPorUsuarioNombre, opt => opt.MapFrom(src => src.CreadoPorUsuario != null ? src.CreadoPorUsuario.UserName : null));

            CreateMap<ComentarioCreacionDto, Comentario>()
                .ForMember(dest => dest.CreadoUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreadoPorUsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ComentarioModificacionDto, Comentario>()
                .ForMember(dest => dest.CreadoUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreadoPorUsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
