using Goke.Core.Engines;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

Console.WriteLine("Hello, World!");

Console.WriteLine("This is a C# program using language version 14.0 and targeting .NET 10.0.");


var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.Create(
        UnicodeRanges.All
        )
};

//var options2 = new JsonSerializerOptions
//{
//    WriteIndented = true,
//    Encoder = JavaScriptEncoder.Create(
//        UnicodeRanges.BasicLatin,
//        UnicodeRanges.Latin1Supplement,
//        UnicodeRanges.LatinExtendedA,
//        UnicodeRanges.LatinExtendedB,
//        UnicodeRanges.MiscellaneousSymbols,
//        UnicodeRanges.MiscellaneousSymbolsAndPictographs,
//        UnicodeRanges.Emoticons,
//        UnicodeRanges.Dingbats,
//        UnicodeRanges.SymbolsAndPictographs,
//        UnicodeRanges.TransportAndMapSymbols,
//        UnicodeRanges.SupplementalSymbolsAndPictographs,
//        UnicodeRanges.All
//    )
//};

string json = JsonSerializer.Serialize(WeatherForecastEngine.Cities.OrderBy(c => c.City), options);

Console.OutputEncoding = System.Text.Encoding.UTF8; // Ensure console can display emojis

Console.WriteLine(json);

File.WriteAllText("cities.json", json, Encoding.UTF8);