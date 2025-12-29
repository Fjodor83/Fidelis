using AutoMapper;
using Fidelity.Domain.Entities;
using Fidelity.Shared.DTOs;

namespace Fidelity.Server
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Transazione -> TransazioneResponse
            CreateMap<Transazione, TransazioneResponse>()
                .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => $"{src.Cliente.Nome} {src.Cliente.Cognome}"))
                .ForMember(dest => dest.CodiceFidelity, opt => opt.MapFrom(src => src.Cliente.CodiceFidelity))
                .ForMember(dest => dest.PuntoVenditaNome, opt => opt.MapFrom(src => src.PuntoVendita.Nome))
                .ForMember(dest => dest.ResponsabileNome, opt => opt.MapFrom(src => src.Responsabile != null ? (src.Responsabile.NomeCompleto ?? src.Responsabile.Username) : "Sistema"))
                .ForMember(dest => dest.TipoTransazione, opt => opt.MapFrom(src => src.Tipo.ToString()));

            // Cliente -> ClienteDettaglioResponse
            CreateMap<Cliente, ClienteDettaglioResponse>()
                .ForMember(dest => dest.PuntoVenditaRegistrazione, opt => opt.MapFrom(src => src.PuntoVenditaRegistrazione != null ? src.PuntoVenditaRegistrazione.Nome : null))
                .ForMember(dest => dest.UltimeTransazioni, opt => opt.Ignore()); // Transazioni loaded separately

            // Cliente -> ClienteResponse
            CreateMap<Cliente, ClienteResponse>()
                .ForMember(dest => dest.NomeCompleto, opt => opt.MapFrom(src => $"{src.Nome} {src.Cognome}"))
                .ForMember(dest => dest.PuntoVenditaRegistrazione, opt => opt.MapFrom(src => src.PuntoVenditaRegistrazione != null ? src.PuntoVenditaRegistrazione.Nome : null))
                .ForMember(dest => dest.PuntoVenditaCodice, opt => opt.MapFrom(src => src.PuntoVenditaRegistrazione != null ? src.PuntoVenditaRegistrazione.Codice : null));

            // Coupon -> CouponDTO
            CreateMap<Coupon, CouponDTO>()
                .ForMember(dest => dest.TipoSconto, opt => opt.MapFrom(src => src.TipoSconto.ToString()));

            // CouponAssegnato -> CouponAssegnatoDTO
            CreateMap<CouponAssegnato, CouponAssegnatoDTO>()
                .ForMember(dest => dest.Codice, opt => opt.MapFrom(src => src.Coupon.Codice))
                .ForMember(dest => dest.Titolo, opt => opt.MapFrom(src => src.Coupon.Titolo))
                .ForMember(dest => dest.Descrizione, opt => opt.MapFrom(src => src.Coupon.Descrizione))
                .ForMember(dest => dest.ValoreSconto, opt => opt.MapFrom(src => src.Coupon.ValoreSconto))
                .ForMember(dest => dest.TipoSconto, opt => opt.MapFrom(src => src.Coupon.TipoSconto.ToString()))
                .ForMember(dest => dest.DataScadenza, opt => opt.MapFrom(src => src.Coupon.DataScadenza));

            // Transazione -> RecentActivityDTO
            CreateMap<Transazione, RecentActivityDTO>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.DataTransazione))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => "Punti"))
                .ForMember(dest => dest.Descrizione, opt => opt.MapFrom(src => $"{src.PuntiAssegnati} punti"))
                .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => $"{src.Cliente.Nome} {src.Cliente.Cognome}"))
                .ForMember(dest => dest.PuntoVendita, opt => opt.MapFrom(src => src.PuntoVendita.Nome))
                .ForMember(dest => dest.Responsabile, opt => opt.MapFrom(src => src.Responsabile != null ? src.Responsabile.Username : "Sistema"));

            // CouponAssegnato -> RecentActivityDTO
            CreateMap<CouponAssegnato, RecentActivityDTO>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.DataUtilizzo))
                .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => "Coupon"))
                .ForMember(dest => dest.Descrizione, opt => opt.MapFrom(src => $"Riscattato {src.Coupon.Codice}"))
                .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => $"{src.Cliente.Nome} {src.Cliente.Cognome}"))
                .ForMember(dest => dest.PuntoVendita, opt => opt.MapFrom(src => "N/A"))
                .ForMember(dest => dest.Responsabile, opt => opt.MapFrom(src => "N/A"));

            // PuntoVendita -> PuntoVenditaResponse
            CreateMap<PuntoVendita, PuntoVenditaResponse>()
                .ForMember(dest => dest.NumeroClienti, opt => opt.MapFrom(src => src.ClientiRegistrati.Count(c => c.Attivo)));

            // PuntoVendita -> PuntoVenditaBasicInfo
            CreateMap<PuntoVendita, PuntoVenditaBasicInfo>();

            // Responsabile -> ResponsabileDetailResponse
            CreateMap<Responsabile, ResponsabileDetailResponse>()
                .ForMember(dest => dest.PuntiVendita, opt => opt.MapFrom(src => src.ResponsabilePuntiVendita.Select(rp => rp.PuntoVendita)));
        }
    }
}
