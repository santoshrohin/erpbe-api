namespace ErpBE.Application.Interfaces;

/// <summary>
/// Writes entries to LOG_MASTER — the same table the legacy system uses.
/// Provides a unified cross-system audit trail during coexistence.
///
/// Matches legacy CommonClasses.WriteLog(FormName, Event, DocName, DocNo,
/// DocCode, CompId, UserName, UserCode).
/// </summary>
public interface IActivityLogService
{
    /// <summary>
    /// Logs a mutating operation (INSERT / UPDATE / DELETE) to LOG_MASTER.
    /// Failures are swallowed and logged to Serilog — a log failure must never
    /// abort the business operation.
    /// </summary>
    Task WriteLogAsync(
        int    companyId,
        string source,    // controller / feature name  e.g. "CustomerMaster"
        string @event,    // "INSERT" | "UPDATE" | "DELETE"
        string docName,   // human label e.g. "Customer Master"
        string docNo,     // document number string
        long   docCode,   // primary key of affected record
        string userName,
        int    userCode,
        string? ipAddress   = null,
        int?   companyCode  = null,
        CancellationToken cancellationToken = default);
}
