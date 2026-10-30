using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;

public sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<int, Vehicle> _vehicles = new();
    private int _nextId = 1;

    public Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<Vehicle> result = _vehicles.Values.OrderBy(vehicle => vehicle.Id).Select(Copy).ToList();

            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<Vehicle>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IReadOnlyList<Vehicle> result = _vehicles.Values.Where(vehicle => vehicle.IsAvailable).
                OrderBy(vehicle => vehicle.Capacity).Select(Copy).ToList();

            return Task.FromResult(result);
        }
    }

    public Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_vehicles.TryGetValue(id, out var vehicle) ? Copy(vehicle) : null);
        }
    }

    public Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(vehicle);

        if(string.IsNullOrWhiteSpace(vehicle.Name))
            throw new ArgumentNullException("El nombre del vehículo es obligatorio");
        if (vehicle.Capacity <= 0)
            throw new ArgumentOutOfRangeException("La capacidad del vehículo debe ser mayor a cero");

        lock (_sync)
        {
            var stored = Copy(vehicle);
            stored.Id = _nextId++;

            _vehicles.Add(stored.Id, stored);

            return Task.FromResult(Copy(stored));
        }
    }
    public Task<bool> UpdateAsync(Vehicle vehicle, string changedByUser, string? reason = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(vehicle);

        if (string.IsNullOrWhiteSpace(vehicle.Name))
            throw new ArgumentNullException("El nombre del vehiculo es obligatorio");

        if (vehicle.Capacity <= 0)
            throw new ArgumentException("La capacidad del vehiculo debe ser mayor a cero");

        if (string.IsNullOrWhiteSpace(changedByUser))
        {
            throw new ArgumentException("El usuario que realiza el cambio es obligatorio", nameof(changedByUser));
        }

        lock (_sync)
        {
            if (!_vehicles.ContainsKey(vehicle.Id))
                return Task.FromResult(false);

            _vehicles[vehicle.Id] = Copy(vehicle);
            return Task.FromResult(true);
        }
    }

    private static Vehicle Copy(Vehicle vehicle)
    {
        return new Vehicle
        {
            Id = vehicle.Id,
            Name = vehicle.Name,
            Capacity = vehicle.Capacity,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            IsAvailable = vehicle.IsAvailable,
        };
    }
}