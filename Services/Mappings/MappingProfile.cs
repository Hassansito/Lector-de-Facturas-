using AutoMapper;
using Models.DTO;
using Models.Entities;

namespace Services.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
           

            CreateMap<CreateClienteDTO, ClienteEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NumeroCliente, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                //.ForMember(dest => dest.Entidad, opt => opt.Ignore())
                .ForMember(dest => dest.Provincia, opt => opt.Ignore())
                .ForMember(dest => dest.Ciclo, opt => opt.MapFrom(src => true));

            CreateMap<UpdateClienteDTO, ClienteEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // No actualizar el Id
                .ForMember(dest => dest.NumeroCliente, opt => opt.Ignore()) // Ignorar campos no incluidos en el DTO
                .ForMember(dest => dest.Ciclo, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Monto, opt => opt.Ignore()); // O MontoTotalFacturas

            CreateMap<ClienteEntity, ResponseDTO>()
            // Mapeo explícito en caso de que los nombres de propiedades difieran
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NumeroCliente, opt => opt.MapFrom(src => src.NumeroCliente))
            .ForMember(dest => dest.Ciclo, opt => opt.MapFrom(src => src.Ciclo))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.Monto, opt => opt.MapFrom(src => src.Monto))
            .ForMember(dest => dest.Entidad, opt => opt.MapFrom(src => src.Entidad))
            .ForMember(dest => dest.Provincia, opt => opt.MapFrom(src => src.Provincia));
        }
    }
}
