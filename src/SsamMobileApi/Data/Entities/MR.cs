using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SsamMobileApi.Data.Entities;

/// <summary>
/// Maps [SIGSSAMI_OS].[MTL].[MR]. Column names, types and nullability replicate
/// the production schema exactly — including apparent typos (OrderRecjectedBy).
/// Do not rename members to "fix" them; the API must stay in sync with the DB.
/// </summary>
[Table("MR", Schema = "MTL")]
public class MR
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MRId { get; set; }

    public int TenantId { get; set; }

    [Required, MaxLength(50)]
    public string RequestType { get; set; } = string.Empty;

    public DateOnly MonthYear { get; set; }

    public int CustomerId { get; set; }

    public int LocationId { get; set; }

    [MaxLength(50)]
    public string? RefId { get; set; }

    public DateOnly? RefDate { get; set; }

    public string? Notes { get; set; }

    public string? Field1 { get; set; }
    public string? Field2 { get; set; }
    public string? Field3 { get; set; }
    public string? Field4 { get; set; }
    public string? Field5 { get; set; }
    public string? Field6 { get; set; }
    public string? Field7 { get; set; }
    public string? Field8 { get; set; }

    public int Status { get; set; }

    public int RequestedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RequestedOn { get; set; }

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public int UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedOn { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? ACKStatus { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? OrderCreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderCreatedDate { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? OrderSubmittedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderSubmittedOn { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? OrderRecjectedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderRejectedDate { get; set; }

    public long? ShipToId { get; set; }

    public bool? IsOnlineOrder { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? DistRemarks { get; set; }

    public int? InvLocId { get; set; }

    [Column(TypeName = "varchar(20)")]
    public string? Dist_MSP_Approval_Status { get; set; }

    [Column(TypeName = "varchar(1000)")]
    public string? SalesUserRemarks { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? FreightTerms { get; set; }

    public ICollection<MRDetail> MRDetails { get; set; } = new List<MRDetail>();
}
