
using LOGAPI_CON.DTOs;
using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;

public class RoutePlanningService
{
    private readonly IMapsService _mapsService;
    private readonly IOrderRepository _orderRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public RoutePlanningService(
        IMapsService mapsService,
        IOrderRepository orderRepository,
        IVehicleRepository vehicleRepository)
    {
        _mapsService = mapsService;
        _orderRepository = orderRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<PlanRouteResponse> CreatePlanAsync(
        PlanRouteRequest request, CancellationToken cancellationToken = default)
    {
        var response = new PlanRouteResponse
        {
            InitialDate = request.Date.Date
        };

        var allOrders = await _orderRepository.GetAllAsync(cancellationToken);

        var pendingOrders = allOrders
            .Where(order =>
                order.Status == OrderStatus.PendingRouting &&
                (order.ScheduledDate == null ||
                 order.ScheduledDate.Value.Date <= request.Date.Date))
            .OrderByDescending(order => order.IsPriority)
            .ThenBy(order => order.CreatedAt)
            .ToList();

        var availableVehicles = await _vehicleRepository.GetAvailableAsync(cancellationToken);

        foreach (var vehicle in availableVehicles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (pendingOrders.Count == 0 || 
                response.Orders.Sum(order => order.Weight) >= request.MaxTonnagePerDay)
            {
                break;
            }

            var routeOrders = new List<DeliveryOrder>();
            decimal actualWeight = 0;

            foreach (var order in pendingOrders.ToList())
            {
                if (response.Orders.Sum(order => order.Weight) + routeOrders.Sum(order => order.Weight)
                    >= request.MaxTonnagePerDay)
                {
                    break;
                }

                if (order.Weight <= 0)
                    continue;

                var fitsWeight = actualWeight + order.Weight <= vehicle.Capacity;

                if (!fitsWeight)
                    continue;

                routeOrders.Add(order);
                actualWeight += order.Weight;
                pendingOrders.Remove(order);
            }

            if (routeOrders.Count == 0)
                continue;

            var addresses = routeOrders.Select(order => order.DeliveryAddress).ToList();

            var routeResult = new RouteResult();

            if (addresses.Count > 1)
            {
                routeResult = await _mapsService.CalculateRouteAsync(addresses);
            }

            var routePlan = new RoutePlan
            {
                VehicleId = vehicle.Id,
                VehicleName = vehicle.Name,
                VehicleCapacity = vehicle.Capacity,
                TotalWeight = actualWeight,
                RemainingCapacity = vehicle.Capacity - actualWeight,
                Distance = routeResult.TotalDistance,
                Duration = routeResult.Duration,
                Stops = routeOrders
                    .Select((order, index) => new RouteStop
                    {
                        Sequence = index + 1,
                        OrderId = order.Id,
                        Customer = order.Customer,
                        DeliveryAddress = order.DeliveryAddress,
                        Weight = order.Weight
                    })
                    .ToList()
            };

            response.RoutePlans.Add(routePlan);
            response.Orders.AddRange(routeOrders);
        }

        foreach (var order in response.Orders)
        {
            order.Status = OrderStatus.Routed;
            order.ScheduledDate = request.Date.Date;

            var updated = await _orderRepository.UpdateAsync(order, cancellationToken);

            if (!updated)
            {
                throw new InvalidOperationException($"No se pudo actualizar el pedido {order.Id}.");
            }
        }

        response.UnassignedOrders = pendingOrders;

        return response;
    }
}
