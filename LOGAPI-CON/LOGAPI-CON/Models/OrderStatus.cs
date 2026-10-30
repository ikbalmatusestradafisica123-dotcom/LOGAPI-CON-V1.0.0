namespace LOGAPI_CON.Models;
public enum OrderStatus
{
    Created = 1,
    PendingValidation = 2,
    Planned = 3,
    InPreparation = 4,
    Assigned = 5,
    ReadyToLoad = 6,
    Loaded = 7,
    InTransit = 8,
    Delivered = 9,
    Rescheduled = 10,
    Cancelled = 11,
    Returned = 12,
    Special = 13,
    Validated = 14,
    PendingInventoryValidation = 15,
    InventoryValidated = 16,
    PendingLogisticsValidation = 17,
    LogisticsValidated = 18,
    PendingRouting = 19,
    Routed = 20,
    SupervisorValidated = 21,
    PartiallyDelivered = 22,
    ClientValidated = 23,
    Finished = 24
}
