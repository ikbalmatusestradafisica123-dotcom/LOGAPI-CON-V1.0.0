using LOGAPI_CON.Data;
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/integrations/mas-admin")]
[Authorize(Roles =
    AppRoles.IT + "," +
    AppRoles.Owner + "," +
    AppRoles.Administrator + "," +
    AppRoles.Logistics + "," +
    AppRoles.Sales)]
public sealed class MasAdminIntegrationController : ControllerBase
{
    private readonly AppDbContext _db;

    public MasAdminIntegrationController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("orders")]
    public async Task<ActionResult<DeliveryOrder>> ImportOrder(
        [FromBody] ImportMasAdminOrderRequest request,
        CancellationToken cancellationToken)
    {
        var externalFolio =
            request.ExternalFolio.Trim();

        var existingOrder = await _db.DeliveryOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                order =>
                    order.ExternalFolio == externalFolio,
                cancellationToken);

        if (existingOrder is not null)
        {
            return Ok(new
            {
                created = false,
                duplicated = true,
                message =
                    "El folio ya estaba registrado.",
                order = existingOrder
            });
        }

        var order = new DeliveryOrder
        {
            ExternalFolio = externalFolio,
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

        _db.DeliveryOrders.Add(order);

        try
        {
            await _db.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentOrder =
                await _db.DeliveryOrders.AsNoTracking().FirstOrDefaultAsync(
                        item => item.ExternalFolio == externalFolio,
                        cancellationToken);

            if (concurrentOrder is not null)
            {
                return Ok(new
                {created = false, duplicated = true, message =
                        "El folio ya estaba registrado.",
                    order = concurrentOrder });
            }

            throw;
        }

        return Created( $"/api/Orders/{order.Id}", new
            {created = true, duplicated = false, message = 
                "Pedido importado correctamente.", order });
    }
}
