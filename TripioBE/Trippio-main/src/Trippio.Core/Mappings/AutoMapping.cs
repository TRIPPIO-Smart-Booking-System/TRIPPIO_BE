using AutoMapper;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Models.Auth;
using Trippio.Core.Models.System;

namespace Trippio.Core.Mappings
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            // Auth mappings
            CreateMap<AppUser, Models.Auth.UserDto>();
            CreateMap<RegisterRequest, AppUser>();

            // System mappings  
            CreateMap<AppUser, Models.System.UserDto>();
            CreateMap<Models.System.CreateUserRequest, AppUser>();
            CreateMap<Models.System.UpdateUserRequest, AppUser>();
        }
    }
}
