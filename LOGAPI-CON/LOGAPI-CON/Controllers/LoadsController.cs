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
[Route("api/[controller]")]
[Authorize(Roles =
    AppRoles.IT + "," +
    AppRoles.Owner + "," +
    AppRoles.Administrator + "," +
    AppRoles.Logistics + "," +
    AppRoles.Supervisor)]



public sealed class LoadsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LoadsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Load>>> GetAll(
        CancellationToken cancellationToken)
    {
        var loads = await _db.Loads
            .AsNoTracking()
            .Include(load => load.Orders)
            .OrderByDescending(load => load.RouteDate)
            .ThenByDescending(load => load.Id)
            .ToListAsync(cancellationToken);

        return Ok(loads);
    }

    [HttpPost("assign")]
    public async Task<ActionResult<Load>> Assign(
        [FromBody] AssignLoadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OrderIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "Debes seleccionar al menos un pedido."
            });
        }

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(
                item => item.Id == request.VehicleId,
                cancellationToken);

        if (vehicle is null)
        {
            return NotFound(new
            {
                message = "El vehículo no existe."
            });
        }

        if (!vehicle.IsAvailable)
        {
            return Conflict(new
            {
                message = "El vehículo no está disponible."
            });
        }

        var requestedIds = request.OrderIds
            .Distinct()
            .ToList();

        var orders = await _db.DeliveryOrders
            .Where(order => requestedIds.Contains(order.Id))
            .ToListAsync(cancellationToken);

        if (orders.Count != requestedIds.Count)
        {
            return NotFound(new
            {
                message = "Uno o más pedidos no existen."
            });
        }

        var invalidOrders = orders
            .Where(order =>
                order.Status != OrderStatus.PendingRouting &&
                order.Status != OrderStatus.Routed)
            .ToList();

        if (invalidOrders.Count > 0)
        {
            return Conflict(new
            {
                message =
                    "Solo puedes asignar pedidos pendientes de ruteo " +
                    "o ya ruteados."
            });
        }







        var partialWeights = await _db.PartialDeliveries
    .Where(item => requestedIds.Contains(item.OrderId))
    .GroupBy(item => item.OrderId)
    .Select(group => new
    {
        OrderId = group.Key,
        DeliveredWeight = group.Sum(
            item => item.DeliveredWeight)
    })
    .ToDictionaryAsync(
        item => item.OrderId,
        item => item.DeliveredWeight,
        cancellationToken);

        var existingAssignments = await _db.LoadOrders
            .Where(item => requestedIds.Contains(item.OrderId))
            .Where(item =>
                !_db.PartialDeliveries.Any(partial =>
                    partial.OrderId == item.OrderId))
            .AnyAsync(cancellationToken);

        if (existingAssignments)
        {
            return Conflict(new
            {
                message =
                    "Uno o más pedidos ya están asignados a una carga."
            });
        }



        var remainingWeights = orders.ToDictionary(
            order => order.Id,
            order =>
            {
                var delivered = partialWeights.TryGetValue(
                    order.Id,
                    out var value)
                        ? value
                        : 0m;

                return order.Weight - delivered;
            });

        if (remainingWeights.Any(item => item.Value <= 0))
        {
            return Conflict(new
            {
                message =
                    "Uno de los pedidos no tiene peso pendiente."
            });
        }

        var totalWeight = remainingWeights.Values.Sum();

        if (totalWeight > vehicle.Capacity)
        {
            return Conflict(new
            {
                message =
                    $"El peso total ({totalWeight}) supera la " +
                    $"capacidad del vehículo ({vehicle.Capacity})."
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

        var load = new Load
        {
            VehicleId = vehicle.Id,
            RouteDate = request.RouteDate.Date,
            TotalWeight = totalWeight,
            CreatedByUserId = userId,
            Status = LoadStatus.Planned,
            Orders = orders
                .Select((order, index) => new LoadOrder
                {
                    OrderId = order.Id,
                    Sequence = index + 1,
                    AssignedWeight = remainingWeights[order.Id],
                })
                .ToList(),
        };

        foreach (var order in orders)
        {
            order.Status = OrderStatus.Routed;
            order.ScheduledDate = request.RouteDate.Date;
        }

        _db.Loads.Add(load);

        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = load.Id },
            load);
    }
}
