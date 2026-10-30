namespace LOGAPI_CON.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
