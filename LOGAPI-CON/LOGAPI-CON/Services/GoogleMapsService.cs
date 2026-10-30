using System.Net.Http.Json;
using System.Text.Json;
using LOGAPI_CON.Models;

namespace LOGAPI_CON.Services;

public class GoogleMapsService : IMapsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleMapsService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new InvalidOperationException("La API Key de Google Maps no está configurada.");
    }

    public async Task<RouteResult> CalculateRouteAsync(
        List<string> addresses)
    {
        if (addresses.Count < 2)
        {
            throw new ArgumentException("Se requieren al menos 2 direcciones para planear una ruta.");
        }

        var origin = addresses.First();
        var destination = addresses.Last();
        var intermediates = addresses.Skip(1).SkipLast(1).Select(address => new
            {
                address = address
            }).ToList();

        var requestBody = new
        {
            origin = new
            {
                address = origin
            },

            destination = new
            {
                address = destination
            },

            intermediates,

            travelMode = "DRIVE",
            routingPreference = "TRAFFIC_AWARE",
            optimizeWaypointOrder = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post,"https://routes.googleapis.com/directions/v2:computeRoutes");

        request.Headers.Add("X-Goog-Api-Key",_apiKey);

        request.Headers.Add("X-Goog-FieldMask", "routes.duration," + "routes.distanceMeters," + 
            "routes.polyline.encodedPolyline," + "routes.optimizedIntermediateWaypointIndex");

        request.Content = JsonContent.Create(requestBody);
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Error al calcular la ruta: " + content);
        }

        using var json = JsonDocument.Parse(content);
        var routes = json.RootElement.GetProperty("routes");

        if (routes.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Google Maps no devolvió ninguna ruta.");
        }

        var route = routes[0];
        var distanceMeters = route.GetProperty("distanceMeters").GetDouble();
        var durationText = route.GetProperty("duration").GetString();
        var durationSeconds = ParseDurationSeconds(durationText);
        string polyline = string.Empty;

        if (route.TryGetProperty("polyline", out var polylineElement))
        {
            if (polylineElement.TryGetProperty("encodedPolyline", out var encodedPolyline))
            {
                polyline = encodedPolyline.GetString() ?? string.Empty;
            }
        }

        return new RouteResult
        {
            TotalDistance = distanceMeters / 1000.0,
            Duration = durationSeconds / 60.0,
            Polyline = polyline
        };
    }

    private static double ParseDurationSeconds(
        string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
        {
            return 0;
        }

        duration = duration.Trim().TrimEnd('s');
        return double.Parse(duration, System.Globalization.CultureInfo.InvariantCulture);
    }
}