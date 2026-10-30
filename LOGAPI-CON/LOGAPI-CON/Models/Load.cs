namespace LOGAPI_CON.Models;

public enum LoadStatus
{
    Planned = 1,
    ReadyToLoad = 2,
    Loaded = 3,
    InTransit = 4,
    Finished = 5,
    Cancelled = 6
}

public sealed class Load
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public DateTime RouteDate { get; set; }

    public LoadStatus Status { get; set; } =
        LoadStatus.Planned;

    public decimal TotalWeight { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public string CreatedByUserId { get; set; } =
        string.Empty;

    public List<LoadOrder> Orders { get; set; } = [];
}
