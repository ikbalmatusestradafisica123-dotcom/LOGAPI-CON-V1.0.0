using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LOGAPI_CON.Services;

public sealed class SqlVehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _dbContext;

    public SqlVehicleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .OrderBy(vehicle => vehicle.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.IsAvailable)
            .OrderBy(vehicle => vehicle.Capacity)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                vehicle => vehicle.Id == id,
                cancellationToken);
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        ValidateVehicle(vehicle);

        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vehicle;
    }

    public async Task<bool> UpdateAsync(Vehicle vehicle, string changedByUser, string? reason = null, CancellationToken cancellationToken = default)
    {
        ValidateVehicle(vehicle);

        if (string.IsNullOrWhiteSpace(changedByUser))
        {
            throw new ArgumentException("El usuario que realiza el cambio es obligatorio", nameof(changedByUser));
        }

        var existingVehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(
                current => current.Id == vehicle.Id,
                cancellationToken);

        if (existingVehicle is null)
        {
            return false;
        }

        var changedAt = DateTime.UtcNow;
        var normalizedReason = string.IsNullOrWhiteSpace(reason)? null : reason.Trim();

        void RegisterChange(string propertyName, string? oldValue, string? newValue)
        {
            if(oldValue == newValue)
            {
                return;
            }

            _dbContext.VehicleHistories.Add(new VehicleHistory
            {
                VehicleId = existingVehicle.Id,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeByUser = changedByUser,
                ChangedAt = changedAt,
                Reason = normalizedReason
            });
        }

        RegisterChange(nameof(Vehicle.Name), existingVehicle.Name, vehicle.Name);

        RegisterChange(nameof(Vehicle.Capacity), 
            existingVehicle.Capacity.ToString(System.Globalization.CultureInfo.InvariantCulture), 
            vehicle.Capacity.ToString(System.Globalization.CultureInfo.InvariantCulture));

        RegisterChange(nameof(Vehicle.Brand), existingVehicle.Brand, vehicle.Brand);

        RegisterChange(nameof(Vehicle.Model), existingVehicle.Model, vehicle.Model);

        RegisterChange(nameof(Vehicle.LicensePlate), existingVehicle.LicensePlate, vehicle.LicensePlate);

        RegisterChange(nameof(Vehicle.IsAvailable), existingVehicle.IsAvailable.ToString(), vehicle.IsAvailable.ToString());



        existingVehicle.Name = vehicle.Name;
        existingVehicle.Capacity = vehicle.Capacity;
        existingVehicle.Brand = vehicle.Brand;
        existingVehicle.Model = vehicle.Model;
        existingVehicle.LicensePlate = vehicle.LicensePlate;
        existingVehicle.IsAvailable = vehicle.IsAvailable;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (string.IsNullOrWhiteSpace(vehicle.Name))
        {
            throw new ArgumentException("El nombre del vehículo es obligatorio.", nameof(vehicle));
        }

        if (string.IsNullOrWhiteSpace(vehicle.Model))
        {
            throw new ArgumentException("El model del veículo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(vehicle.Brand))
        {
            throw new ArgumentException("La marca del vehículo es obligatoria");
        }

        if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
        {
            throw new ArgumentException("Las placas del vehiculo son obligatorias");
        }

        if (vehicle.Capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vehicle), "La capacidad del vehículo debe ser mayor a cero.");
        }

        vehicle.Name = vehicle.Name.Trim();
        vehicle.Brand = vehicle.Brand.Trim();
        vehicle.Model = vehicle.Model.Trim();
        vehicle.LicensePlate = vehicle.LicensePlate.Trim().ToUpperInvariant();
    }
}