using MediatR;

namespace ErpBE.Application.LineMasters.Commands
{
    public class CreateLineCommand : IRequest<int>
    {
        public string LineName { get; set; }
    }
}
