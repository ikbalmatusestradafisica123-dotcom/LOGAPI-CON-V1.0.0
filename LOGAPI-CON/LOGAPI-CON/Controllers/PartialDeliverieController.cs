using System.Security.Claims;
using LOGAPI_CON.Data;
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/orders/{orderId:int}/partial-deliveries")]
[Authorize(Roles =
    AppRoles.Driver + "," +
    AppRoles.Supervisor + "," +
    AppRoles.Logistics + "," +
    AppRoles.Administrator + "," +
    AppRoles.Owner + "," +
    AppRoles.IT)]
public sealed class PartialDeliveriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PartialDeliveriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PartialDelivery>>> GetAll(
        int orderId,
        CancellationToken cancellationToken)
    {
        var orderExists = await _db.DeliveryOrders
            .AnyAsync(
                order => order.Id == orderId,
                cancellationToken);

        if (!orderExists)
        {
            return NotFound(new
            {
                message = "El pedido no existe."
            });
        }

        var deliveries = await _db.PartialDeliveries
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .OrderBy(item => item.RegisteredAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(deliveries);
    }

    [HttpPost]
    public async Task<ActionResult<PartialDelivery>> Create(
        int orderId,
        [FromBody] CreatePartialDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _db.DeliveryOrders
            .FirstOrDefaultAsync(
                item => item.Id == orderId,
                cancellationToken);

        if (order is null)
        {
            return NotFound(new
            {
                message = "El pedido no existe."
            });
        }

        if (order.Status != OrderStatus.InTransit)
        {
            return Conflict(new
            {
                message =
                    "Solo se puede registrar una parcialidad " +
                    "cuando el pedido está en ruta."
            });
        }

        if (request.DeliveredWeight >= order.Weight)
        {
            return BadRequest(new
            {
                message =
                    "La entrega parcial debe ser menor que el " +
                    "peso total del pedido."
            });
        }

        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var alreadyDelivered = await _db.PartialDeliveries
            .Where(item => item.OrderId == orderId)
            .SumAsync(
                item => item.DeliveredWeight,
                cancellationToken);

        var pendingWeight =
            order.Weight -
            alreadyDelivered -
            request.DeliveredWeight;

        if (pendingWeight <= 0)
        {
            return BadRequest(new
            {
                message =
                    "La entrega parcial deja un peso pendiente " +
                    "no válido."
            });
        }

        if (request.RescheduledDate is null)
        {
            return BadRequest(new
            {
                message =
                    "Debes indicar la nueva fecha de entrega."
            });
        }

        var partialDelivery = new PartialDelivery
        {
            OrderId = orderId,
            OrderedWeight = order.Weight,
            DeliveredWeight = request.DeliveredWeight,
            PendingWeight = pendingWeight,
            Reason = request.Reason.Trim(),
            Comments = string.IsNullOrWhiteSpace(request.Comments)
                ? null
                : request.Comments.Trim(),
            RescheduledDate =
                request.RescheduledDate.Value.Date,
            RegisteredByUserId = userId.Trim(),
            RegisteredAtUtc = DateTime.UtcNow,
        };

        order.Status = OrderStatus.PartiallyDelivered;
        order.ScheduledDate =
            request.RescheduledDate.Value.Date;

        _db.PartialDeliveries.Add(partialDelivery);

        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            PreviousStatus = OrderStatus.InTransit,
            NewStatus = OrderStatus.PartiallyDelivered,
            ChangedByUserId = userId.Trim(),
            ChangedAt = DateTime.UtcNow,
            Reason = request.Reason.Trim(),
        });

        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { orderId },
            partialDelivery);
    }
}
