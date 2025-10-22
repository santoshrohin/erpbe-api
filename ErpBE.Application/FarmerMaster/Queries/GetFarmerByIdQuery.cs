using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetFarmerByIdQuery : IRequest<FarmerDto?>
    {
        public int Id { get; }

        public GetFarmerByIdQuery(int id)
        {
            Id = id;
        }
    }

}
