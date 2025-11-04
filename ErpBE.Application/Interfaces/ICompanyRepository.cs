namespace ErpBE.Application.Interfaces;

/// <summary>
/// Repository interface for Company operations
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Gets distinct active companies
    /// </summary>
    Task<List<CompanyDto>> GetActiveCompaniesAsync();
}

public class CompanyDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

