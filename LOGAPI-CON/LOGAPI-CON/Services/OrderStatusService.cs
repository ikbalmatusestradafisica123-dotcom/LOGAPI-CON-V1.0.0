
using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Services;

public sealed class OrderStatusService
{
    private readonly AppDbContext _db;

    public OrderStatusService(AppDbContext db)
    {
        _db = db;
    }

    public bool CanTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus == OrderStatus.Special || newStatus == OrderStatus.Special)
        {
            return false;
        }

        var newFlowTransition = (currentStatus, newStatus) switch
        {
            (OrderStatus.Created, OrderStatus.Validated) => true,
            (OrderStatus.Validated, OrderStatus.PendingInventoryValidation) => true,
            (OrderStatus.PendingInventoryValidation, OrderStatus.InventoryValidated) => true,
            (OrderStatus.InventoryValidated, OrderStatus.PendingLogisticsValidation) => true,
            (OrderStatus.PendingLogisticsValidation, OrderStatus.LogisticsValidated) => true,
            (OrderStatus.LogisticsValidated, OrderStatus.PendingRouting) => true,
            (OrderStatus.PendingRouting, OrderStatus.Routed) => true,
            (OrderStatus.Routed, OrderStatus.ReadyToLoad) => true,
            (OrderStatus.ReadyToLoad, OrderStatus.Loaded) => true,
            (OrderStatus.Loaded, OrderStatus.SupervisorValidated) => true,
            (OrderStatus.SupervisorValidated, OrderStatus.InTransit) => true,
            (OrderStatus.InTransit, OrderStatus.ClientValidated) => true,
            (OrderStatus.ClientValidated, OrderStatus.Delivered) => true,
            (OrderStatus.Delivered, OrderStatus.Finished) => true,
            (OrderStatus.InTransit, OrderStatus.PartiallyDelivered) => true,
            (OrderStatus.PartiallyDelivered, OrderStatus.Rescheduled) => true,
            (OrderStatus.Rescheduled, OrderStatus.PendingRouting) => true,
            (OrderStatus.Returned, OrderStatus.PendingRouting) => true,
            (OrderStatus.Created, OrderStatus.Cancelled) => true,
            (OrderStatus.Validated, OrderStatus.Cancelled) => true,
            (OrderStatus.PendingInventoryValidation, OrderStatus.Cancelled) => true,
            (OrderStatus.InventoryValidated, OrderStatus.Cancelled) => true,
            (OrderStatus.PendingLogisticsValidation, OrderStatus.Cancelled) => true,
            (OrderStatus.PendingRouting, OrderStatus.Cancelled) => true,
            (OrderStatus.Routed, OrderStatus.Cancelled) => true,
            (OrderStatus.ReadyToLoad, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (newFlowTransition)
        {
            return true;
        }

        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Created, OrderStatus.PendingValidation) => true,
            (OrderStatus.PendingValidation, OrderStatus.Planned) => true,
            (OrderStatus.Planned, OrderStatus.InPreparation) => true,
            (OrderStatus.InPreparation, OrderStatus.Assigned) => true,
            (OrderStatus.Assigned, OrderStatus.ReadyToLoad) => true,
            (OrderStatus.ReadyToLoad, OrderStatus.Loaded) => true,
            (OrderStatus.Loaded, OrderStatus.InTransit) => true,
            (OrderStatus.InTransit, OrderStatus.Delivered) => true,
            (OrderStatus.Delivered, OrderStatus.Rescheduled) => true,
            (OrderStatus.Rescheduled, OrderStatus.Cancelled) => true,
            (OrderStatus.Cancelled, OrderStatus.Returned) => true,
            _ => false
        };
    }





    public bool CanRollbackOnStage(OrderStatus currentStatus, OrderStatus targetStatus)
    {

        var newFlowRollback = (currentStatus, targetStatus)switch
        {
            (OrderStatus.Validated, OrderStatus.Created) => true,
            (OrderStatus.PendingInventoryValidation, OrderStatus.Validated) => true,
            (OrderStatus.InventoryValidated, OrderStatus.PendingInventoryValidation) => true,
            (OrderStatus.PendingLogisticsValidation, OrderStatus.InventoryValidated) => true,
            (OrderStatus.LogisticsValidated, OrderStatus.PendingLogisticsValidation) => true,
            (OrderStatus.PendingRouting, OrderStatus.LogisticsValidated) => true,
            (OrderStatus.Routed, OrderStatus.PendingRouting) => true,
            (OrderStatus.ReadyToLoad, OrderStatus.Routed) => true,
            (OrderStatus.Loaded, OrderStatus.ReadyToLoad) => true,
            (OrderStatus.SupervisorValidated, OrderStatus.Loaded) => true,
            (OrderStatus.InTransit, OrderStatus.SupervisorValidated) => true,
            (OrderStatus.ClientValidated, OrderStatus.InTransit) => true,
            (OrderStatus.Delivered, OrderStatus.ClientValidated) => true,
            (OrderStatus.Finished, OrderStatus.Delivered) => true,
            (OrderStatus.PartiallyDelivered, OrderStatus.InTransit) => true,
            (OrderStatus.Rescheduled, OrderStatus.PartiallyDelivered) => true,

            _ => false
        };

        if (newFlowRollback)
        {
            return true;
        }


        return (currentStatus, targetStatus) switch
        {
            (OrderStatus.PendingValidation,OrderStatus.Created) => true,
            (OrderStatus.Planned,OrderStatus.PendingValidation) => true,
            (OrderStatus.InPreparation,OrderStatus.Planned) => true,
            (OrderStatus.Assigned,OrderStatus.InPreparation) => true,
            (OrderStatus.ReadyToLoad,OrderStatus.Assigned) => true,
            (OrderStatus.Loaded,OrderStatus.ReadyToLoad) => true,
            (OrderStatus.InTransit,OrderStatus.Loaded) => true,
            (OrderStatus.Delivered,OrderStatus.InTransit) => true,
            (OrderStatus.Rescheduled,OrderStatus.Delivered) => true,
            (OrderStatus.Cancelled,OrderStatus.Rescheduled) => true,
            (OrderStatus.Returned,OrderStatus.Cancelled) => true,

            _ => false
        };
    }

    public bool IsValidDirectCorrection(OrderStatus targetStatus, string? ticketReference)
    {
        return Enum.IsDefined(targetStatus)
            && targetStatus != OrderStatus.Special
            && !string.IsNullOrWhiteSpace(ticketReference);
    }

    public async Task<StatusChangeResult> ChangeStatusAsync(
        int orderId,
        OrderStatus newStatus,
        string reason,
        string changedByUserId,
        string? ticketReference = null,
        bool isRollback = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 3)
        {
            return StatusChangeResult.Invalid("El motivo debe tener al menos 3 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(changedByUserId))
        {
            return StatusChangeResult.Invalid("No se pudo identificar al usuario.");
        }

        if (!Enum.IsDefined(newStatus))
        {
            return StatusChangeResult.Invalid("El estado no existe");
        }

        var order = await _db.DeliveryOrders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

        if (order is null)
        {
            return StatusChangeResult.NotFound();
        }

        var allowed = isRollback? CanRollbackOnStage(order.Status, newStatus)
            : CanTransition(order.Status, newStatus);

        if (!allowed)
        {
            return StatusChangeResult.Conflict($"No se permite cambiar de {order.Status} a {newStatus}.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var previousStatus = order.Status;
        order.Status = newStatus;

        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedByUserId = changedByUserId.Trim(),
            ChangedAt = DateTime.UtcNow,
            TicketReference = string.IsNullOrWhiteSpace(ticketReference)? null : ticketReference.Trim(),
            Reason = reason.Trim()
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return StatusChangeResult.Success(order);
    }
}

public sealed record StatusChangeResult(
    bool Succeeded,
    bool OrderNotFound,
    bool IsInvalid,
    string? Error,
    DeliveryOrder? Order)
{
    public static StatusChangeResult Success(DeliveryOrder order) =>
        new(true, false, false, null, order);

    public static StatusChangeResult NotFound() =>
        new(false, true, false, "No se encontró el pedido.", null);

    public static StatusChangeResult Invalid(string error) =>
        new(false, false, true, error, null);

    public static StatusChangeResult Conflict(string error) =>
        new(false, false, false, error, null);
}
