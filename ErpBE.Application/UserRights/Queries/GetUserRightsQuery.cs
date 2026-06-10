using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserRights.Queries;

public class GetUserRightsQuery : IRequest<IEnumerable<UserRightDto>>
{
    public int UserCode { get; set; }
}
