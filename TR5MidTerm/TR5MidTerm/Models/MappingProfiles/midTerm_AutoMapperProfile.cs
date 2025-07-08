using AutoMapper;
using TR5MidTerm.Models;
using TR5MidTerm.Models.TR5MidTermViewModels;
 

namespace TR5MidTerm.Models.MappingProfiles
{
    public class midTerm_AutoMapperProfile : Profile
    {
        public midTerm_AutoMapperProfile()
        {
            // 主檔 Mapping
            //CreateMap<承租人檔, 承租人檔VM>();
            //CreateMap<承租人檔VM, 承租人檔>();
            CreateMap<承租人檔, 承租人檔DisplayViewModel>();
            CreateMap<承租人檔DisplayViewModel, 承租人檔>();
            CreateMap<承租人檔CreateViewModel, 承租人檔>()
            .ForMember(dest => dest.承租人, opt => opt.Ignore())
            .ForMember(dest => dest.統一編號, opt => opt.Ignore())
            .ForMember(dest => dest.行動電話, opt => opt.Ignore())
            .ForMember(dest => dest.電子郵件, opt => opt.Ignore());
            ;
            CreateMap<承租人檔EditViewModel, 承租人檔>();
            CreateMap<承租人檔,承租人檔EditViewModel>();


        }
    }
}
