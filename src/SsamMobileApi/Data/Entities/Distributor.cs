namespace SsamMobileApi.Data.Entities;

/// <summary>
/// Example entity. After you scaffold from the real database this file will be
/// replaced by generated classes that match your actual tables.
/// </summary>
public class Distributor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Region { get; set; }
    public bool IsActive { get; set; }
}
