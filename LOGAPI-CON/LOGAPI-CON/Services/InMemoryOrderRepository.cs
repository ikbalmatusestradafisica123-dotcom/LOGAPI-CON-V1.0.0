using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services
{
    public sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly object _sync = new object();
        private readonly Dictionary<int, DeliveryOrder> _orders = new();
        private int _nextId = 1;

        public Task<IReadOnlyList<DeliveryOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                IReadOnlyList<DeliveryOrder> result = _orders.Values.OrderByDescending(order => order.CreatedAt)
                    .Select(Copy).ToList();

                return Task.FromResult(result);
            }
        }

        public Task<DeliveryOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                var found = _orders.TryGetValue(id, out var order);

                return Task.FromResult(found && order is not null ? Copy(order) : null);
            }
        }

        public Task<DeliveryOrder?> CreateAsync(DeliveryOrder order, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync) 
            {
                var stored = Copy(order);
                stored.Id = _nextId++;

                if(stored.CreatedAt == default)
                {
                    stored.CreatedAt = DateTime.UtcNow;
                }

                _orders.Add(stored.Id, stored);
                return Task.FromResult(Copy(stored));
            }
        }

        private static DeliveryOrder Copy(DeliveryOrder order)
        {
            return new DeliveryOrder
            {
                Id = order.Id,
                Customer = order.Customer,
                Weight = order.Weight,
                DeliveryAddress = order.DeliveryAddress,
                Sector = order.Sector,
                CreatedAt = order.CreatedAt,
                ScheduledDate = order.ScheduledDate,
                Status = order.Status,
                IsPriority = order.IsPriority,
                PriorityReason = order.PriorityReason
            };
        }

        public Task<bool> UpdateAsync(DeliveryOrder order, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ArgumentNullException.ThrowIfNull(order);

            lock (_sync)
            {
                if(!_orders.ContainsKey(order.Id))
                    return Task.FromResult(false);

                _orders[order.Id] = Copy(order);

                return Task.FromResult(true);
            }
        }
    }
}
