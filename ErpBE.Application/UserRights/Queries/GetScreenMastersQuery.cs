using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UserRights.Queries;

public class GetScreenMastersQuery : IRequest<IEnumerable<ScreenMasterDto>> { }
