using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.FarmerItemMaster
{
    public class GetAllFarmerItemsQuery : IRequest<List<FarmerItemDto>> { }
}
