using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Logs.Commands
{
    public class CleanupOldLogsCommandHandler : IRequestHandler<CleanupOldLogsCommand, int>
    {
        private readonly ILogsRepository _logsRepository;

        public CleanupOldLogsCommandHandler(ILogsRepository logsRepository)
        {
            _logsRepository = logsRepository;
        }

        public async Task<int> Handle(CleanupOldLogsCommand request, CancellationToken cancellationToken)
        {
            return await _logsRepository.CleanupOldLogsAsync(request.DaysToKeep);
        }
    }
}
