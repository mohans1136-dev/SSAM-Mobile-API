using SSAMMobileApp.Data.Entities;

namespace SSAMMobileApp.Models;

/// <summary>Entity -> DTO projections. Kept in one place so the shape stays consistent.</summary>
public static class MrMappings
{
    public static MrDetailDto ToDto(this MRDetail d) => new(
        d.MRDetailId, d.MRId, d.RequestType, d.MonthYear, d.IsComputed, d.CustomerId, d.LocationId,
        d.RefId, d.RefDate, d.Product, d.ItemId, d.InvLocId,
        d.AQ1, d.AQ2, d.AQ3, d.Q1, d.Q2, d.Q3, d.TotalQty, d.STP,
        d.DelDate, d.Remarks, d.Instructions, d.Length, d.Quantity, d.L1, d.L2, d.L3, d.IsBoard,
        d.Field1, d.Field2, d.Field3, d.Field4, d.Field5, d.Field6, d.Field7, d.Field8,
        d.STAG_HDR_ID, d.STAG_LINE_ID, d.Status, d.RequestedBy, d.RequestedOn,
        d.CreatedBy, d.CreatedOn, d.UpdatedBy, d.UpdatedOn,
        d.ACKStatus, d.ModStatus, d.ModUpdatedBy, d.ModUpdatedDate,
        d.OrderCreatedBy, d.OrderCreatedDate, d.OrderSubmittedBy, d.OrderSubmittedOn,
        d.OrderRecjectedBy, d.OrderRejectedDate, d.ShipToId, d.IsOnlineOrder,
        d.HSNCode, d.STPFromOS, d.Price, d.PriceOnOS, d.SalesUserRemarks,
        d.AcceptedPrice, d.MSPPrice, d.Distributor_Accepted_Remarks, d.MSP_Accepted_Remarks, d.ProcessedVia);

    public static MrDto ToDto(this MR m) => new(
        m.MRId, m.TenantId, m.RequestType, m.MonthYear, m.CustomerId, m.LocationId, m.RefId, m.RefDate, m.Notes,
        m.Field1, m.Field2, m.Field3, m.Field4, m.Field5, m.Field6, m.Field7, m.Field8,
        m.Status, m.RequestedBy, m.RequestedOn, m.CreatedBy, m.CreatedOn, m.UpdatedBy, m.UpdatedOn,
        m.ACKStatus, m.OrderCreatedBy, m.OrderCreatedDate, m.OrderSubmittedBy, m.OrderSubmittedOn,
        m.OrderRecjectedBy, m.OrderRejectedDate, m.ShipToId, m.IsOnlineOrder, m.DistRemarks, m.InvLocId,
        m.Dist_MSP_Approval_Status, m.SalesUserRemarks, m.FreightTerms,
        m.MRDetails.OrderBy(d => d.MRDetailId).Select(d => d.ToDto()).ToList());
}
