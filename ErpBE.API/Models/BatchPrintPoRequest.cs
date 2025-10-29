using ErpBE.Application.DTOs;

namespace ErpBE.API.Models;

/// <summary>
/// Request model for batch printing Customer POs
/// </summary>
public class BatchPrintPoRequest
{
    public int CompanyId { get; set; }
    public List<PoPrintRequest> Pos { get; set; } = new();
}

/// <summary>
/// Single PO print request
/// </summary>
public class PoPrintRequest
{
    public int PoCode { get; set; }
    public PoCopyType CopyType { get; set; } = PoCopyType.Original;
}


