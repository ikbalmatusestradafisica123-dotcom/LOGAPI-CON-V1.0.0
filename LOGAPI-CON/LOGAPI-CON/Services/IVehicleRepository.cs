using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetAvailableAsync(CancellationToken cancellationToken = default);

    Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Vehicle vehicle, string changedByUser, string? reason = null, CancellationToken cancellationToken = default);
}