using AutoMapper;
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels;

namespace Compass.Data.Data.AutoMapper
{
    public class AutoMapperUserProfile : Profile
    {
        public AutoMapperUserProfile()
        {
            CreateMap<AppUser, RegisterUserVM>();
            CreateMap<RegisterUserVM, AppUser>().ForMember(dst => dst.UserName, act => act.MapFrom(src => src.Email));
            CreateMap<AppUser, GetAllUsersVM>();
            CreateMap<AppUser, UserProfileVM>();
        }
    }
}
