namespace GokeSharedCore.MapApp;

public sealed class DirectionRouteMarker
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string TransportMode { get; set; } = string.Empty;

    public double StartLatitude { get; set; }

    public double StartLongitude { get; set; }

    public double EndLatitude { get; set; }

    public double EndLongitude { get; set; }

    public double CurrentLatitude { get; set; }

    public double CurrentLongitude { get; set; }

    public double Progress { get; set; }

    public string? Description { get; set; }

    public string? Timestamp { get; set; }

    public string? MarkerColor { get; set; }

    public string? MarkerShape { get; set; }

    public string? MarkerInitials { get; set; }
}
