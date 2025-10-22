using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetAllFarmersQuery : IRequest<List<FarmerDto>> { }
}
