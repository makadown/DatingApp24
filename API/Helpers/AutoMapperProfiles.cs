using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember( dest => dest.PhotoUrl,
                    opt => opt.MapFrom(src => 
                        src.Photos.FirstOrDefault(x => x.IsMain).Url ) )
                .ForMember( dest => dest.Age,
                    opt=> opt.MapFrom( src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<Photo, PhotoForApprovalDto>()
                .ForMember( dest => dest.Username, 
                            opt => opt.MapFrom( src => 
                                        src.AppUser.UserName) );
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember( dest => dest.SenderPhotoUrl,
                            opt => opt.MapFrom(src => 
                                    src.Sender.Photos.FirstOrDefault( x => x.IsMain).Url) )
                .ForMember( dest => dest.RecipientPhotoUrl,
                            opt => opt.MapFrom(src => 
                                    src.Recipient.Photos.FirstOrDefault( x => x.IsMain).Url) );
            // Esto lo que hace es que cuando se envía el formato de fecha al cliente, se regresa
            // con formato local (con Z al final).
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        }
    }
}