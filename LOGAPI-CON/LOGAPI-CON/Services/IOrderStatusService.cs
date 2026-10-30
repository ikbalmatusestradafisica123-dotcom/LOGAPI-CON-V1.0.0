using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;
public interface IOrderStatusService
{
    Task<StatusChangeResult> ChangeStatusAsync(
        int orderId,
        OrderStatus newStatus,
        string reason,
        string? ticketReference,
        string changeByUserId,
        CancellationToken cancellationToken = default
        );

    public sealed record StatusChangeResult(
        bool OrderFound,
        bool TransitionAllowed,
        DeliveryOrder? Order
        );
}
