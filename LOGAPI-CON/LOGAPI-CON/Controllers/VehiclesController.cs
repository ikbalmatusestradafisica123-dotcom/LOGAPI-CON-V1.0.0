using LOGAPI_CON.Models;
using LOGAPI_CON.Services;
using LOGAPI_CON.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicles;

    public VehiclesController(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    [HttpGet]
    [Authorize(Roles =
    AppRoles.Administrator + "," +
    AppRoles.IT + "," +
    AppRoles.Owner + "," +
    AppRoles.Logistics + "," +
    AppRoles.Supervisor)]


    public async Task<ActionResult<IReadOnlyList<Vehicle>>> GetAll(CancellationToken cancellationToken)
    {
        var vehicles = await _vehicles.GetAllAsync(cancellationToken);
        return Ok(vehicles);
    }

    [HttpGet("Available")]
    [Authorize(Roles =
    AppRoles.Administrator + "," +
    AppRoles.IT + "," +
    AppRoles.Owner + "," +
    AppRoles.Logistics + "," +
    AppRoles.Supervisor)]


    public async Task<ActionResult<IReadOnlyList<Vehicle>>> GetAvailable(CancellationToken cancellationToken)
    {
        var vehicles = await _vehicles.GetAvailableAsync(cancellationToken);
        return Ok(vehicles);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles =
    AppRoles.Administrator + "," +
    AppRoles.IT + "," +
    AppRoles.Owner + "," +
    AppRoles.Logistics + "," +
    AppRoles.Supervisor)]


    public async Task<ActionResult<Vehicle>> GetById(int id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        if (vehicle is null)
        {
            return NotFound(new { message = "No fué posible encontrar el vehículo buscado, intente buscar con otro ID diferente" });
        }
        return Ok(vehicle);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Administrator + "," + AppRoles.IT + "," + AppRoles.Owner + "," + AppRoles.Logistics)]

    public async Task<ActionResult<Vehicle>> Create([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            LicensePlate = request.LicensePlate.Trim().ToUpperInvariant(),
            IsAvailable = request.IsAvailable
        };

        var createdVehicle = await _vehicles.CreateAsync(vehicle, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdVehicle.Id }, createdVehicle);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Administrator + "," + AppRoles.IT + "," + AppRoles.Owner + "," + AppRoles.Logistics)]

    public async Task<ActionResult<Vehicle>> Update(int id, [FromBody] UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado" });
        }

        var vehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        if(vehicle is null)
        {
            return NotFound(new { message = "No fué posible encontrar el vehículo buscado, intente buscar con un ID diferente" });
        }

        vehicle.Name = request.Name.Trim();
        vehicle.Capacity = request.Capacity;
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.LicensePlate = request.LicensePlate.Trim().ToUpperInvariant();
        vehicle.IsAvailable = request.IsAvailable;

        var updated = await _vehicles.UpdateAsync(vehicle, userId, request.Reason, cancellationToken);

        if (!updated)
        {
            return NotFound(new { message = "No fue posible actualizar el vehículo" });
        }

        var updatedVehicle = await _vehicles.GetByIdAsync(id, cancellationToken);

        return Ok(updatedVehicle);
    }
}

public sealed class CreateVehicleRequest
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal Capacity { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public sealed class UpdateVehicleRequest
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal Capacity { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
