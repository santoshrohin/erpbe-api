using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserRights.Commands;

public class SaveUserRightsCommand : IRequest<bool>
{
    public int                    UserCode { get; set; }
    public List<UserRightRequest> Rights   { get; set; } = new();
}

public class CopyUserRightsCommand : IRequest<bool>
{
    public int FromUserCode { get; set; }
    public int ToUserCode   { get; set; }
}

public class DeleteUserRightsCommand : IRequest<bool>
{
    public int UserCode { get; set; }
}
