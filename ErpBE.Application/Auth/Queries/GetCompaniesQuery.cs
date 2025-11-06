using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Auth.Queries
{
    public class GetCompaniesQuery : IRequest<List<CompanyDto>>
    {
    }
}

