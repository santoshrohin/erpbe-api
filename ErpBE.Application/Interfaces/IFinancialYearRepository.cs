namespace ErpBE.Application.Interfaces;

/// <summary>
/// Repository interface for Financial Year operations
/// </summary>
public interface IFinancialYearRepository
{
    /// <summary>
    /// Gets financial years for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of financial years with formatted display text</returns>
    Task<List<FinancialYearDto>> GetFinancialYearsByCompanyIdAsync(int companyId);
}

public class FinancialYearDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int FinancialYearCode { get; set; }
    public string OpeningDate { get; set; } = string.Empty;
    public string ClosingDate { get; set; } = string.Empty;
}

