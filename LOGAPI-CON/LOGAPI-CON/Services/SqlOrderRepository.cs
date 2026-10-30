using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Services;

public sealed class SqlOrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public SqlOrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DeliveryOrder>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeliveryOrders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeliveryOrder?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeliveryOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<DeliveryOrder?> CreateAsync(
        DeliveryOrder order,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.CreatedAt == default)
        {
            order.CreatedAt = DateTime.UtcNow;
        }

        _dbContext.DeliveryOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<bool> UpdateAsync(
        DeliveryOrder order,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        var existingOrder = await _dbContext.DeliveryOrders
            .FirstOrDefaultAsync(
                current => current.Id == order.Id,
                cancellationToken);

        if (existingOrder is null)
        {
            return false;
        }

        existingOrder.Customer = order.Customer;
        existingOrder.Weight = order.Weight;
        existingOrder.DeliveryAddress = order.DeliveryAddress;
        existingOrder.Sector = order.Sector;
        existingOrder.ScheduledDate = order.ScheduledDate;
        existingOrder.Status = order.Status;
        existingOrder.IsPriority = order.IsPriority;
        existingOrder.PriorityReason = order.PriorityReason;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}