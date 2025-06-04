using AutoMapper;
using EnterpriseScheduler.Models;
using EnterpriseScheduler.Models.Common;
using EnterpriseScheduler.Models.DTOs.Meetings;
using EnterpriseScheduler.Models.DTOs.Users;

namespace EnterpriseScheduler.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserRequest, User>();
        CreateMap<User, UserResponse>();

        CreateMap<MeetingRequest, Meeting>();
        CreateMap<Meeting, MeetingResponse>()
            .ForMember(dest => dest.ParticipantIds, opt => opt.MapFrom(src => src.Participants.Select(p => p.Id)));

        CreateMap(typeof(PaginatedResult<>), typeof(PaginatedResult<>))
            .ConvertUsing(typeof(PaginatedResultConverter<,>));
    }
}

public class PaginatedResultConverter<TSource, TDestination> : ITypeConverter<PaginatedResult<TSource>, PaginatedResult<TDestination>>
{
    public PaginatedResult<TDestination> Convert(PaginatedResult<TSource> source, PaginatedResult<TDestination> destination, ResolutionContext context)
    {
        return new PaginatedResult<TDestination>
        {
            Items = context.Mapper.Map<IEnumerable<TDestination>>(source.Items),
            TotalCount = source.TotalCount,
            PageNumber = source.PageNumber,
            PageSize = source.PageSize
        };
    }
}
