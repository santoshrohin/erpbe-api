using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

public interface IUserRightRepository
{
    Task<IEnumerable<ScreenMasterDto>> GetScreensAsync(CancellationToken ct = default);
    Task<IEnumerable<UserRightDto>>    GetUserRightsAsync(int userCode, CancellationToken ct = default);
    Task<bool>                         UpsertUserRightsAsync(int userCode, IEnumerable<UserRightRequest> rights, CancellationToken ct = default);
    Task<bool>                         CopyRightsAsync(int fromUserCode, int toUserCode, CancellationToken ct = default);
    Task<bool>                         DeleteUserRightsAsync(int userCode, CancellationToken ct = default);
}
