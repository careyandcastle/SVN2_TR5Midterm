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
            CreateMap<承租人檔, 承租人檔VM>()
                // ✅ 將 byte[] → 解密為 string
                //.ForMember(dest => dest.承租人顯示, opt => opt.MapFrom(src => CustomSqlFunctions.DecryptToString(src.承租人)))
                //.ForMember(dest => dest.統一編號顯示, opt => opt.MapFrom(src => CustomSqlFunctions.DecryptToString(src.統一編號)))
                //.ForMember(dest => dest.行動電話顯示, opt => opt.MapFrom(src => CustomSqlFunctions.DecryptToString(src.行動電話)))
                //.ForMember(dest => dest.電子郵件顯示, opt => opt.MapFrom(src => CustomSqlFunctions.DecryptToString(src.電子郵件)))

                // ✅ 身分別名稱（從 navigation property）
                //.ForMember(dest => dest.身分別名稱, opt => opt.MapFrom(src => src.身分別編號Navigation.身分別))
    //            .ForMember(dest => dest.身分別名稱,
    //opt => opt.MapFrom(src => src.身分別編號Navigation != null ? src.身分別編號Navigation.身分別 : null))

                ;

            CreateMap<承租人檔VM, 承租人檔>();
        }
    }
}
