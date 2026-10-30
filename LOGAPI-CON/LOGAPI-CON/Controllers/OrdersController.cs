
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using LOGAPI_CON.Services;
using LOGAPI_CON.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orders;
    private readonly OrderStatusService _orderStatusService;

    public OrdersController(IOrderRepository orders, OrderStatusService orderStatusService)
    {
        _orders = orders;
        _orderStatusService = orderStatusService;
    }

    [HttpGet]
    [Authorize(Roles =
        AppRoles.Administrator + "," +
        AppRoles.Owner + "," +
        AppRoles.IT + "," +
        AppRoles.Logistics + "," +
        AppRoles.Supervisor + "," +
        AppRoles.Sales + "," +
        AppRoles.Inventory + "," +
        AppRoles.Purchasing + "," +
        AppRoles.Warehouse)]
    public async Task<ActionResult<IReadOnlyList<DeliveryOrder>>> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await _orders.GetAllAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    //[Authorize(Roles =
    //    AppRoles.Administrator + "," +
    //    AppRoles.IT + "," +
    //    AppRoles.Logistics + "," +
    //    AppRoles.Owner + "," +
    //    AppRoles.Supervisor + "," +
    //    AppRoles.Sales + "," +
    //    AppRoles.Inventory + "," +
    //    AppRoles.Warehouse)]
    public async Task<ActionResult<DeliveryOrder>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound(new
            {
                message = "No se encontró el pedido."
            });
        }

        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles =
        AppRoles.Administrator + "," +
        AppRoles.IT + "," +
        AppRoles.Owner + "," +
        AppRoles.Logistics + "," +
        AppRoles.Supervisor + "," +
        AppRoles.Sales)]
    public async Task<ActionResult<DeliveryOrder>> Create(
        [FromBody] CreateDeliveryOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new DeliveryOrder
        {
            Customer = request.Customer.Trim(),
            Weight = request.Weight,
            DeliveryAddress = request.DeliveryAddress.Trim(),
            Sector = request.Sector.Trim(),
            CreatedAt = DateTime.UtcNow,
            ScheduledDate = request.ScheduledDate,
            Status = OrderStatus.Created,
            IsPriority = false,
            PriorityReason = null
        };

        var createdOrder = await _orders.CreateAsync(order, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles =
        AppRoles.IT + "," +
        AppRoles.Administrator + "," +
        AppRoles.Logistics + "," +
        AppRoles.Owner + "," + 
        AppRoles.Supervisor + "," + 
        AppRoles.Sales)]
    public async Task<ActionResult<DeliveryOrder>> Update(int id,
        [FromBody] UpdateDeliveryOrderRequest request, CancellationToken cancellationToken)
    {
        var existingOrder = await _orders.GetByIdAsync(id, cancellationToken);

        if (existingOrder is null)
        {
            return NotFound(new
            {
                message = "No se encontró el pedido."
            });
        }

        existingOrder.Customer = request.Customer.Trim();
        existingOrder.Weight = request.Weight;
        existingOrder.DeliveryAddress = request.DeliveryAddress.Trim();
        existingOrder.Sector = request.Sector.Trim();
        existingOrder.ScheduledDate = request.ScheduledDate;

        var updated = await _orders.UpdateAsync(existingOrder, cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "No se encontró el pedido."
            });
        }

        return Ok(existingOrder);
    }

    [HttpPatch("{id:int}/estatus")]
    [Authorize(Roles =
        AppRoles.IT + "," +
        AppRoles.Logistics + "," +
        AppRoles.Inventory + "," +
        AppRoles.Driver + "," +
        AppRoles.Administrator + "," + 
        AppRoles.Owner + "," + 
        AppRoles.Supervisor)]

    public async Task<IActionResult> ChangeStatus(int id, 
        [FromBody] ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "No se pudo identificar el usuario autenticado"
            });
        }

        if(request.IsRollback && !User.IsInRole(AppRoles.Administrator) 
            && !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT) 
            && !User.IsInRole(AppRoles.Inventory))
        {
            return Forbid();
        }

        if(request.NewStatus == OrderStatus.InventoryValidated && !User.IsInRole(AppRoles.Administrator)
            && !User.IsInRole(AppRoles.Inventory) && !User.IsInRole(AppRoles.IT) 
            && !User.IsInRole(AppRoles.Owner))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.LogisticsValidated &&
            !User.IsInRole(AppRoles.Logistics) && !User.IsInRole(AppRoles.Administrator) &&
            !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.PendingRouting && !User.IsInRole(AppRoles.Logistics) &&
            !User.IsInRole(AppRoles.Administrator) && !User.IsInRole(AppRoles.Owner) &&
            !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.ReadyToLoad && !User.IsInRole(AppRoles.Administrator) 
            && !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT) 
            && !User.IsInRole(AppRoles.Logistics))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.Loaded && !User.IsInRole(AppRoles.Logistics) &&
            !User.IsInRole(AppRoles.Administrator) && !User.IsInRole(AppRoles.Owner) &&
            !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.SupervisorValidated && !User.IsInRole(AppRoles.Supervisor) &&
            !User.IsInRole(AppRoles.Administrator) && !User.IsInRole(AppRoles.Owner) 
            && !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.InTransit && !User.IsInRole(AppRoles.Driver) &&
            !User.IsInRole(AppRoles.Supervisor) && !User.IsInRole(AppRoles.Administrator) &&
            !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }


        if (request.NewStatus == OrderStatus.ClientValidated && !User.IsInRole(AppRoles.Driver) &&
            !User.IsInRole(AppRoles.Supervisor) && !User.IsInRole(AppRoles.Administrator) &&
            !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.Delivered && !User.IsInRole(AppRoles.Driver) &&
            !User.IsInRole(AppRoles.Supervisor) && !User.IsInRole(AppRoles.Administrator) &&
            !User.IsInRole(AppRoles.Owner) && !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.Finished && !User.IsInRole(AppRoles.Supervisor) &&
            !User.IsInRole(AppRoles.Administrator) && !User.IsInRole(AppRoles.Owner) &&
            !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }

        if (request.NewStatus == OrderStatus.Rescheduled && !User.IsInRole(AppRoles.Driver) && !User.IsInRole(AppRoles.Logistics) && 
            !User.IsInRole(AppRoles.Supervisor) && !User.IsInRole(AppRoles.Administrator) && !User.IsInRole(AppRoles.Owner) && 
            !User.IsInRole(AppRoles.IT))
        {
            return Forbid();
        }


        var result = await _orderStatusService.ChangeStatusAsync(
            orderId : id,
            newStatus : request.NewStatus,
            reason : request.Reason,
            changedByUserId : userId,
            ticketReference : request.TicketReference,
            isRollback: request.IsRollback,
            cancellationToken : cancellationToken
            );

        if (result.OrderNotFound)
        {
            return NotFound(new {message = result.Error});
        }

        if (result.IsInvalid)
        {
            return BadRequest(new {message = result.Error});
        }

        if (!result.Succeeded)
        {
            return Conflict(new {message = result.Error});
        }

        return Ok(result.Order);
    }

    [HttpGet("{id:int}/historial")]
    [Authorize(Roles =
        AppRoles.IT + "," +
        AppRoles.Administrator + "," + 
        AppRoles.Owner + "," +
        AppRoles.Logistics + "," +
        AppRoles.Supervisor )]

    public async Task<ActionResult<IReadOnlyList<OrderStatusHistory>>> 
        GetHistory(int id, [FromServices] LOGAPI_CON.Data.AppDbContext db, CancellationToken cancellationToken)
    {
        var orderExists = await db.DeliveryOrders.AnyAsync(x => x.Id == id, cancellationToken);

        if (!orderExists)
        {
            return NotFound(new { message = $"No se encontro el pedido con número de orden {id}" });
        }

        var history = await db.OrderStatusHistories.AsNoTracking().Where(x => x.OrderId == id).
            OrderBy(x => x.ChangedAt).ThenBy(x => x.Id).ToListAsync(cancellationToken);

        return Ok(history);
    }
}
