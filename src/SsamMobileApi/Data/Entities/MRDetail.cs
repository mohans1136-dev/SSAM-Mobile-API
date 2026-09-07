using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SsamMobileApi.Data.Entities;

/// <summary>
/// Maps [SIGSSAMI_OS].[MTL].[MRDetail]. Column names, types and nullability
/// replicate the production schema exactly — including apparent typos
/// (OrderRecjectedBy). Do not rename members to "fix" them.
/// </summary>
[Table("MRDetail", Schema = "MTL")]
public class MRDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MRDetailId { get; set; }

    public int MRId { get; set; }

    [Required, MaxLength(50)]
    public string RequestType { get; set; } = string.Empty;

    public DateOnly MonthYear { get; set; }

    public bool IsComputed { get; set; }

    public int CustomerId { get; set; }

    public int LocationId { get; set; }

    [MaxLength(50)]
    public string? RefId { get; set; }

    public DateOnly? RefDate { get; set; }

    [Required, MaxLength(1000)]
    public string Product { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public int? InvLocId { get; set; }

    [Column(TypeName = "decimal(10, 2)")] public decimal? AQ1 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? AQ2 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? AQ3 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? Q1 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? Q2 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? Q3 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? TotalQty { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? STP { get; set; }

    public DateOnly? DelDate { get; set; }

    public string? Remarks { get; set; }
    public string? Instructions { get; set; }

    [Column(TypeName = "decimal(10, 2)")] public decimal? Length { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? Quantity { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? L1 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? L2 { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? L3 { get; set; }

    public bool IsBoard { get; set; }

    public string? Field1 { get; set; }
    public string? Field2 { get; set; }
    public string? Field3 { get; set; }
    public string? Field4 { get; set; }
    public string? Field5 { get; set; }
    public string? Field6 { get; set; }
    public string? Field7 { get; set; }
    public string? Field8 { get; set; }

    public int? STAG_HDR_ID { get; set; }
    public int? STAG_LINE_ID { get; set; }

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

    [Column(TypeName = "varchar(20)")] public string? ACKStatus { get; set; }
    [Column(TypeName = "varchar(20)")] public string? ModStatus { get; set; }
    [Column(TypeName = "varchar(20)")] public string? ModUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModUpdatedDate { get; set; }

    [Column(TypeName = "varchar(1000)")] public string? OrderCreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderCreatedDate { get; set; }

    [Column(TypeName = "varchar(1000)")] public string? OrderSubmittedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderSubmittedOn { get; set; }

    [Column(TypeName = "varchar(1000)")] public string? OrderRecjectedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderRejectedDate { get; set; }

    public long? ShipToId { get; set; }

    public bool? IsOnlineOrder { get; set; }

    [Column(TypeName = "varchar(50)")] public string? HSNCode { get; set; }

    [Column(TypeName = "decimal(10, 2)")] public decimal? STPFromOS { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? Price { get; set; }
    [Column(TypeName = "decimal(10, 2)")] public decimal? PriceOnOS { get; set; }

    [Column(TypeName = "varchar(1000)")] public string? SalesUserRemarks { get; set; }

    [Column(TypeName = "decimal(20, 2)")] public decimal? AcceptedPrice { get; set; }
    [Column(TypeName = "decimal(20, 2)")] public decimal? MSPPrice { get; set; }

    [Column(TypeName = "varchar(300)")] public string? Distributor_Accepted_Remarks { get; set; }
    [Column(TypeName = "varchar(300)")] public string? MSP_Accepted_Remarks { get; set; }
    [Column(TypeName = "varchar(50)")] public string? ProcessedVia { get; set; }

    [ForeignKey(nameof(MRId))]
    public MR? MR { get; set; }
}
