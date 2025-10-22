using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.Placement.Queries
{
    public class GetAllPlacementsQuery : IRequest<IEnumerable<PlacementWithDetailsDto>>
    {
    }
}
