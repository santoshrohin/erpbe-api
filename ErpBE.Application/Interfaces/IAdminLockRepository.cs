namespace ErpBE.Application.Interfaces;

public record ActiveLockDto(
    string Module,
    int    RecordId,
    string RecordLabel,
    int?   LockedByUserId,
    DateTime? LockedAt,
    int    MinutesHeld
);

public interface IAdminLockRepository
{
    Task<IEnumerable<ActiveLockDto>> GetActiveLocksAsync();
    Task<bool> ForceUnlockAsync(string module, int recordId);
}
