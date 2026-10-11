namespace GokeApp.Models;

public sealed class MapMarker
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? Description { get; set; }

    public string? Timestamp { get; set; }

    public string? PopupHtml { get; set; }

    public string? MarkerColor { get; set; }

    public string? MarkerShape { get; set; }

    public string? MarkerInitials { get; set; }

    public double? MarkerOpacity { get; set; }
}
