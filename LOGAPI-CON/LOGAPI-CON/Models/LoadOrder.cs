using System.Text.Json.Serialization;

namespace LOGAPI_CON.Models;

public sealed class LoadOrder
{
    public int Id { get; set; }

    public int LoadId { get; set; }

    public int OrderId { get; set; }

    public int Sequence { get; set; }

    public decimal AssignedWeight { get; set; }

    [JsonIgnore]
    public Load Load { get; set; } = null!;
}
