using System.Security.Claims;
using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LOGAPI_CON.Controllers;

[ApiController]
[Route("api/orders/{orderId:int}/evidence")]
[Authorize(Roles =
    AppRoles.Driver + "," +
    AppRoles.Supervisor + "," +
    AppRoles.Logistics + "," +
    AppRoles.Administrator + "," +
    AppRoles.Owner + "," +
    AppRoles.IT)]
public sealed class DeliveryEvidenceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private const long MaxFileSize = 10 * 1024 * 1024;

    public DeliveryEvidenceController(
        AppDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeliveryEvidence>>>
        GetAll(
            int orderId,
            CancellationToken cancellationToken)
    {
        var exists = await _db.DeliveryOrders
            .AnyAsync(
                order => order.Id == orderId,
                cancellationToken);

        if (!exists)
        {
            return NotFound(new
            {
                message = "El pedido no existe."
            });
        }

        var evidence = await _db.DeliveryEvidences
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .OrderByDescending(item => item.UploadedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(evidence);
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<DeliveryEvidence>> Upload(
        int orderId,
        IFormFile file,
        [FromForm] string? evidenceType,
        [FromForm] string? comments,
        CancellationToken cancellationToken)
    {
        var order = await _db.DeliveryOrders
            .FirstOrDefaultAsync(
                item => item.Id == orderId,
                cancellationToken);

        if (order is null)
        {
            return NotFound(new
            {
                message = "El pedido no existe."
            });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Debes seleccionar una fotografía."
            });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new
            {
                message = "La fotografía no puede superar 10 MB."
            });
        }

        if (!AllowedTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "Solo se permiten JPG, PNG o WEBP."
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

        var folder = Path.Combine(
            _environment.ContentRootPath,
            "App_Data",
            "Evidence",
            orderId.ToString());

        Directory.CreateDirectory(folder);

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var fullPath = Path.Combine(
            folder,
            storedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(
                stream,
                cancellationToken);
        }

        var evidence = new DeliveryEvidence
        {
            OrderId = orderId,
            EvidenceType =
                string.IsNullOrWhiteSpace(evidenceType)
                    ? "Delivery"
                    : evidenceType.Trim(),
            OriginalFileName =
                Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            Comments = string.IsNullOrWhiteSpace(comments)
                ? null
                : comments.Trim(),
            UploadedByUserId = userId.Trim(),
            UploadedAtUtc = DateTime.UtcNow
        };

        _db.DeliveryEvidences.Add(evidence);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { orderId },
            evidence);
    }

    [HttpGet("{evidenceId:int}/file")]
    public async Task<IActionResult> Download(
        int orderId,
        int evidenceId,
        CancellationToken cancellationToken)
    {
        var evidence = await _db.DeliveryEvidences
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item =>
                    item.Id == evidenceId &&
                    item.OrderId == orderId,
                cancellationToken);

        if (evidence is null)
        {
            return NotFound();
        }

        var path = Path.Combine(
            _environment.ContentRootPath,
            "App_Data",
            "Evidence",
            orderId.ToString(),
            evidence.StoredFileName);

        if (!System.IO.File.Exists(path))
        {
            return NotFound(new
            {
                message = "El archivo no existe."
            });
        }

        var stream = System.IO.File.OpenRead(path);

        return File(
            stream,
            evidence.ContentType,
            evidence.OriginalFileName);
    }
}
