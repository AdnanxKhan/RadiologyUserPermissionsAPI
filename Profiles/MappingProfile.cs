using AutoMapper;
using UserPermissionsApi.Models;
using UserPermissionsApi.DTOs;

namespace UserPermissionsApi.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserCategoryDTO, UserCategory>();
            CreateMap<UserPermissionDTO, UserPermission>();
        }
    }
}