using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.LineMasters.Queries
{
    public class GetAllLinesQuery : IRequest<List<LineDto>> { }
}
