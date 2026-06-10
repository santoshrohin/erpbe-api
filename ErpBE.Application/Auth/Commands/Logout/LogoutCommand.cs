using MediatR;

namespace ErpBE.Application.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    public int UserCode { get; set; }
}
