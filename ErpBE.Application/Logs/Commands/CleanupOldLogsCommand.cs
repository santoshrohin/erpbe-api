using MediatR;

namespace ErpBE.Application.Logs.Commands
{
    public class CleanupOldLogsCommand : IRequest<int>
    {
        public int DaysToKeep { get; set; } = 30;
    }
}

