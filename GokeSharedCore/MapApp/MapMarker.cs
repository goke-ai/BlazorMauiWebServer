namespace GokeSharedCore.MapApp;


public enum ColorScheme
{
    Light,
    Dark,
    All
};

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

    public double? MarkerValue { get; set; }


    public static readonly Dictionary<string, string> Colors = new()
    {
        { "Red", "#FF0000" },
        { "Blue", "#0000FF" },
        { "Green", "#008000" },
        { "Purple", "#800080" },
        { "Teal", "#008080" },
        { "RoyalBlue", "#4169E1" },
        { "Crimson", "#DC143C" },
        { "Emerald", "#50C878" },
        { "Orange", "#FFA500" },
        { "Magenta", "#FF00FF" },
        { "Turquoise", "#40E0D0" },
        { "Coral", "#FF7F50" },
        { "SkyBlue", "#87CEEB" },
        { "Indigo", "#4B0082" },
        { "Violet", "#EE82EE" },
        { "ForestGreen", "#228B22" },
        { "DarkCyan", "#008B8B" },
        { "SteelBlue", "#4682B4" },
        { "SlateBlue", "#6A5ACD" },
        { "Maroon", "#800000" },
        { "Navy", "#000080" }
    };



    public static string GetColor(string uniqueIdentifier)
    {

       var color = Colors.ElementAt(Math.Abs(uniqueIdentifier.GetHashCode()) % Colors.Count).Value;
       return color;
    }
}
