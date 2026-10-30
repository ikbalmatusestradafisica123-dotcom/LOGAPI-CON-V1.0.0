using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services
{
    public interface IOrderRepository
    {
        Task<IReadOnlyList<DeliveryOrder>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DeliveryOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DeliveryOrder?> CreateAsync(DeliveryOrder order, CancellationToken cancelationToken = default);
        Task<bool> UpdateAsync(DeliveryOrder order, CancellationToken cancellationToken = default);
    }
}
