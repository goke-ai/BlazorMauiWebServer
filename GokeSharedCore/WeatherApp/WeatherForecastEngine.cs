namespace GokeSharedCore.WeatherApp
{


    public static class WeatherForecastEngine
    {
        private const int ForecastIntervalInHour = 1;
        private const int ForecastDays = 14;
        private const int WeatherSystemHours = 6;
        private const int TemperatureSeedSalt = 0;
        private const int SummarySeedSalt = 1;
        private const int HumiditySeedSalt = 2;
        private const int PressureSeedSalt = 3;
        private const int VisibilitySeedSalt = 4;
        private const int AirQualitySeedSalt = 5;
        private const int WindSeedSalt = 6;
        private const int UvSeedSalt = 7;
        private const int TemperatureRangeSeedSalt = 8;
        private const int RainProbabilitySeedSalt = 9;

        public static TimeSpan ForecastInterval => TimeSpan.FromHours(ForecastIntervalInHour);

        private static readonly TimeSpan WeatherSystemInterval = TimeSpan.FromHours(WeatherSystemHours);

        public static int ForecastPeriods => ForecastDays * (24 / ForecastIntervalInHour);

        public static readonly CityWeatherProfile[] Cities =
        [
            new("Abuja", 9.0765, 7.3986, 22, 34, 45, 82, 7, 22, 1006, 1018, 7, 20, 35, 95,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Cairo", 30.0444, 31.2357, 14, 38, 30, 60, 9, 30, 1007, 1020, 8, 24, 35, 110,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Breezy", "🍃"),
                new("Clear", "🌤️")
            ]),
            new("Kuala Lumpur", 3.1390, 101.6869, 24, 32, 78, 96, 5, 14, 1006, 1016, 6, 18, 35, 85,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Rainy", "🌧️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Lagos", 6.5244, 3.3792, 25, 33, 72, 95, 6, 16, 1007, 1018, 8, 24, 55, 120,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅"),
                new("Sunny", "☀️")
            ]),
            new("London", 51.5074, -0.1278, 8, 18, 68, 90, 8, 22, 1008, 1026, 10, 28, 20, 55,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Windy", "💨"),
                new("Cool", "🧥"),
                new("Overcast", "🌥️")
            ]),
            new("Maiduguri", 11.8311, 13.1510, 24, 39, 20, 55, 8, 28, 1004, 1016, 8, 24, 40, 110,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("New York", 40.7128, -74.0060, 6, 24, 48, 82, 8, 26, 1005, 1024, 9, 30, 15, 60,
            [
                new("Cool", "🧥"),
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Showers", "🚿")
            ]),
            new("Oslo", 59.9139, 10.7522, -6, 18, 55, 88, 6, 24, 1002, 1024, 8, 26, 12, 42,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Snow Showers", "🌨️"),
                new("Breezy", "🍃")
            ]),
            new("Paris", 48.8566, 2.3522, 10, 22, 60, 86, 8, 24, 1009, 1025, 7, 22, 18, 48,
            [
                new("Mild", "🙂"),
                new("Cloudy", "☁️"),
                new("Sunny", "☀️"),
                new("Breezy", "🍃"),
                new("Light Rain", "🌦️")
            ]),
            new("Rio de Janeiro", -22.9068, -43.1729, 22, 34, 65, 92, 7, 24, 1006, 1018, 7, 20, 28, 78,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Sydney", -33.8688, 151.2093, 10, 27, 55, 85, 8, 26, 1008, 1024, 9, 28, 12, 45,
            [
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Breezy", "🍃"),
                new("Mild", "🙂")
            ]),
            new("Cape Town", -33.9249, 18.4241, 9, 28, 50, 82, 8, 26, 1008, 1025, 10, 30, 10, 40,
            [
                new("Sunny", "☀️"),
                new("Windy", "💨"),
                new("Partly Cloudy", "⛅"),
                new("Light Rain", "🌦️"),
                new("Mild", "🙂")
            ]),
            new("Mexico City", 19.4326, -99.1332, 8, 27, 40, 75, 8, 24, 1010, 1025, 7, 22, 18, 65,
            [
                new("Sunny", "☀️"),
                new("Mild", "🙂"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Cloudy", "☁️")
            ]),
            new("Kingston", 17.9712, -76.7936, 24, 32, 68, 92, 7, 22, 1007, 1017, 8, 24, 22, 70,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Madrid", 40.4168, -3.7038, 5, 34, 35, 70, 8, 28, 1009, 1025, 7, 22, 14, 52,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Breezy", "🍃"),
                new("Clear", "🌤️")
            ]),
            new("Cardiff", 51.4816, -3.1791, 6, 21, 65, 90, 7, 22, 1004, 1023, 8, 24, 10, 38,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Windy", "💨"),
                new("Cool", "🧥"),
                new("Overcast", "🌥️")
            ]),
            new("Edinburgh", 55.9533, -3.1883, 3, 19, 60, 88, 7, 22, 1003, 1022, 9, 25, 10, 35,
            [
                new("Cool", "🧥"),
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Windy", "💨"),
                new("Overcast", "🌥️")
            ]),
            new("Dublin", 53.3498, -6.2603, 5, 20, 65, 90, 7, 23, 1004, 1023, 8, 24, 10, 36,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Breezy", "🍃"),
                new("Cool", "🧥"),
                new("Overcast", "🌥️")
            ]),
            new("Belfast", 54.5973, -5.9301, 4, 19, 65, 90, 7, 22, 1003, 1022, 8, 25, 10, 36,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Windy", "💨"),
                new("Cool", "🧥"),
                new("Overcast", "🌥️")
            ]),
            new("Beijing", 39.9042, 116.4074, -4, 32, 35, 75, 7, 24, 1008, 1026, 7, 22, 25, 90,
            [
                new("Sunny", "☀️"),
                new("Dry", "🏜️"),
                new("Windy", "💨"),
                new("Cloudy", "☁️"),
                new("Clear", "🌤️")
            ]),
            new("Moscow", 55.7558, 37.6173, -10, 20, 55, 88, 6, 22, 1000, 1023, 8, 24, 12, 45,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Cool", "🧥"),
                new("Overcast", "🌥️")
            ]),
            new("Mumbai", 19.0760, 72.8777, 24, 34, 68, 95, 5, 18, 1002, 1014, 6, 18, 40, 120,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Rainy", "🌧️"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Dubai", 25.2048, 55.2708, 18, 42, 35, 75, 8, 30, 1004, 1018, 7, 24, 30, 95,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃")
            ]),
            new("Riyadh", 24.7136, 46.6753, 10, 43, 20, 55, 8, 30, 1003, 1018, 8, 24, 28, 100,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Clear", "🌤️"),
                new("Windy", "💨")
            ]),
            new("Tehran", 35.6892, 51.3890, 2, 36, 25, 60, 8, 28, 1007, 1023, 7, 22, 22, 78,
            [
                new("Sunny", "☀️"),
                new("Dry", "🏜️"),
                new("Breezy", "🍃"),
                new("Clear", "🌤️"),
                new("Cloudy", "☁️")
            ]),
            new("Jerusalem", 31.7683, 35.2137, 7, 31, 35, 70, 8, 28, 1008, 1024, 7, 20, 16, 58,
            [
                new("Sunny", "☀️"),
                new("Clear", "🌤️"),
                new("Mild", "🙂"),
                new("Breezy", "🍃"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Buenos Aires", -34.6037, -58.3816, 8, 31, 55, 85, 8, 25, 1006, 1023, 8, 24, 16, 52,
            [
                new("Sunny", "☀️"),
                new("Mild", "🙂"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅"),
                new("Windy", "💨")
            ]),
            new("Bogota", 4.7110, -74.0721, 7, 20, 60, 88, 6, 18, 1010, 1025, 6, 18, 12, 42,
            [
                new("Cool", "🧥"),
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Mild", "🙂"),
                new("Overcast", "🌥️")
            ]),
            new("Caracas", 10.4806, -66.9036, 20, 31, 60, 90, 7, 22, 1008, 1018, 6, 18, 18, 62,
            [
                new("Warm", "🌤️"),
                new("Humid", "💧"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅"),
                new("Sunny", "☀️")
            ]),
            new("Toronto", 43.6532, -79.3832, -8, 27, 45, 82, 7, 25, 1002, 1024, 8, 26, 10, 42,
            [
                new("Cold", "🥶"),
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Snow Showers", "🌨️"),
                new("Breezy", "🍃")
            ]),
            new("Montreal", 45.5017, -73.5673, -12, 25, 50, 85, 6, 24, 1001, 1023, 8, 26, 10, 40,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Snow Showers", "🌨️"),
                new("Sunny", "☀️"),
                new("Windy", "💨")
            ]),
            new("Los Angeles", 34.0522, -118.2437, 12, 31, 40, 70, 9, 28, 1009, 1022, 6, 18, 18, 60,
            [
                new("Sunny", "☀️"),
                new("Warm", "🌤️"),
                new("Clear", "🌤️"),
                new("Dry", "🏜️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Dallas", 32.7767, -96.7970, 8, 37, 45, 80, 8, 26, 1004, 1020, 8, 24, 20, 78,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Windy", "💨"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Miami", 25.7617, -80.1918, 21, 34, 65, 92, 7, 22, 1007, 1018, 8, 22, 28, 85,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Sunny", "☀️"),
                new("Showers", "🚿"),
                new("Thunderstorms", "⛈️")
            ]),
            new("Anchorage", 61.2181, -149.9003, -15, 18, 55, 88, 5, 20, 995, 1020, 8, 30, 8, 35,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Breezy", "🍃"),
                new("Clear", "🌤️")
            ]),
            new("Fairbanks", 64.8378, -147.7164, -28, 22, 50, 85, 6, 26, 994, 1022, 6, 24, 6, 28,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Clear", "🌤️"),
                new("Windy", "💨"),
                new("Sunny", "☀️")
            ]),
            new("Reykjavik", 64.1466, -21.9426, -4, 14, 60, 90, 6, 20, 996, 1018, 10, 34, 8, 30,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Windy", "💨"),
                new("Snow Showers", "🌨️")
            ]),
            new("Nuuk", 64.1835, -51.7216, -16, 12, 55, 88, 5, 18, 992, 1018, 10, 32, 6, 24,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("Yellowknife", 62.4540, -114.3718, -26, 20, 50, 84, 6, 24, 994, 1021, 7, 26, 6, 28,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Sunny", "☀️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("Iqaluit", 63.7467, -68.5170, -24, 12, 55, 88, 5, 18, 992, 1018, 9, 30, 6, 24,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("Murmansk", 68.9585, 33.0827, -18, 16, 55, 88, 5, 20, 994, 1020, 8, 28, 8, 30,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Overcast", "🌥️")
            ]),
            new("Novosibirsk", 55.0084, 82.9357, -24, 27, 45, 78, 7, 26, 995, 1022, 7, 24, 10, 40,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Clear", "🌤️")
            ]),
            new("Yakutsk", 62.0355, 129.6755, -38, 24, 45, 78, 7, 28, 994, 1022, 6, 22, 6, 24,
            [
                new("Cold", "🥶"),
                new("Clear", "🌤️"),
                new("Sunny", "☀️"),
                new("Snow Showers", "🌨️"),
                new("Windy", "💨")
            ]),
            new("Vladivostok", 43.1155, 131.8855, -12, 25, 50, 82, 7, 24, 998, 1023, 8, 28, 10, 38,
            [
                new("Cold", "🥶"),
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("Tromso", 69.6492, 18.9553, -10, 15, 60, 90, 5, 18, 995, 1018, 10, 32, 8, 30,
            [
                new("Cold", "🥶"),
                new("Snow Showers", "🌨️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Overcast", "🌥️")
            ]),
            new("Helsinki", 60.1699, 24.9384, -10, 22, 55, 86, 6, 22, 998, 1022, 8, 26, 10, 38,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Snow Showers", "🌨️"),
                new("Breezy", "🍃")
            ]),
            new("Lima", -12.0464, -77.0428, 15, 29, 65, 88, 6, 18, 1009, 1018, 6, 18, 16, 55,
            [
                new("Mild", "🙂"),
                new("Cloudy", "☁️"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿")
            ]),
            new("Santiago", -33.4489, -70.6693, 3, 31, 35, 70, 8, 26, 1008, 1024, 7, 24, 16, 58,
            [
                new("Sunny", "☀️"),
                new("Clear", "🌤️"),
                new("Mild", "🙂"),
                new("Breezy", "🍃"),
                new("Showers", "🚿")
            ]),
            new("Manaus", -3.1190, -60.0217, 24, 33, 72, 96, 5, 16, 1004, 1013, 5, 16, 30, 85,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Rainy", "🌧️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Nairobi", -1.2864, 36.8172, 12, 28, 45, 80, 7, 22, 1009, 1022, 6, 18, 12, 48,
            [
                new("Mild", "🙂"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Breezy", "🍃")
            ]),
            new("Addis Ababa", 8.9806, 38.7578, 9, 25, 40, 78, 8, 24, 1010, 1024, 6, 18, 10, 42,
            [
                new("Mild", "🙂"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Breezy", "🍃")
            ]),
            new("Johannesburg", -26.2041, 28.0473, 4, 28, 35, 72, 8, 26, 1008, 1024, 7, 22, 12, 48,
            [
                new("Sunny", "☀️"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿")
            ]),
            new("Antananarivo", -18.8792, 47.5079, 10, 28, 45, 82, 7, 22, 1008, 1022, 6, 18, 10, 42,
            [
                new("Mild", "🙂"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Breezy", "🍃")
            ]),
            new("Port Louis", -20.1609, 57.5012, 20, 31, 65, 90, 7, 22, 1007, 1018, 7, 20, 14, 48,
            [
                new("Warm", "🌤️"),
                new("Humid", "💧"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅"),
                new("Sunny", "☀️")
            ]),
            new("Honolulu", 21.3069, -157.8583, 22, 31, 62, 86, 8, 24, 1008, 1018, 8, 22, 12, 40,
            [
                new("Warm", "🌤️"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅"),
                new("Showers", "🚿"),
                new("Breezy", "🍃")
            ]),
            new("Suva", -18.1248, 178.4501, 22, 31, 72, 95, 6, 18, 1005, 1015, 7, 20, 12, 42,
            [
                new("Warm", "🌤️"),
                new("Humid", "💧"),
                new("Showers", "🚿"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Chicago", 41.8781, -87.6298, -12, 29, 45, 82, 7, 24, 1000, 1024, 9, 30, 14, 52,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Sunny", "☀️"),
                new("Windy", "💨"),
                new("Snow Showers", "🌨️")
            ]),
            new("Seattle", 47.6062, -122.3321, 2, 24, 60, 90, 7, 22, 1004, 1022, 7, 22, 10, 38,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Mild", "🙂"),
                new("Breezy", "🍃"),
                new("Overcast", "🌥️")
            ]),
            new("San Francisco", 37.7749, -122.4194, 8, 24, 55, 82, 8, 24, 1008, 1022, 8, 24, 12, 42,
            [
                new("Cool", "🧥"),
                new("Cloudy", "☁️"),
                new("Sunny", "☀️"),
                new("Breezy", "🍃"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Denver", 39.7392, -104.9903, -8, 29, 35, 70, 10, 30, 1005, 1025, 8, 26, 12, 48,
            [
                new("Sunny", "☀️"),
                new("Cold", "🥶"),
                new("Clear", "🌤️"),
                new("Windy", "💨"),
                new("Snow Showers", "🌨️")
            ]),
            new("Atlanta", 33.7490, -84.3880, 4, 32, 50, 85, 7, 24, 1004, 1022, 7, 22, 16, 58,
            [
                new("Warm", "🌤️"),
                new("Sunny", "☀️"),
                new("Showers", "🚿"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Washington, D.C.", 38.9072, -77.0369, 1, 31, 48, 84, 7, 24, 1003, 1023, 7, 24, 14, 55,
            [
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Showers", "🚿"),
                new("Breezy", "🍃"),
                new("Hot", "🔥")
            ]),
            new("Phoenix", 33.4484, -112.0740, 10, 43, 18, 45, 10, 32, 1003, 1018, 6, 20, 28, 95,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃")
            ]),
            new("Vancouver", 49.2827, -123.1207, 3, 23, 60, 90, 7, 22, 1003, 1021, 7, 22, 10, 36,
            [
                new("Cloudy", "☁️"),
                new("Light Rain", "🌦️"),
                new("Mild", "🙂"),
                new("Breezy", "🍃"),
                new("Overcast", "🌥️")
            ]),
            new("Calgary", 51.0447, -114.0719, -14, 26, 35, 72, 9, 28, 1002, 1023, 9, 30, 8, 35,
            [
                new("Cold", "🥶"),
                new("Sunny", "☀️"),
                new("Clear", "🌤️"),
                new("Windy", "💨"),
                new("Snow Showers", "🌨️")
            ]),
            new("Ottawa", 45.4215, -75.6972, -14, 27, 45, 82, 7, 24, 1001, 1024, 8, 26, 10, 40,
            [
                new("Cold", "🥶"),
                new("Cloudy", "☁️"),
                new("Snow Showers", "🌨️"),
                new("Sunny", "☀️"),
                new("Breezy", "🍃")
            ]),
            new("Tokyo", 35.6762, 139.6503, 2, 32, 50, 85, 7, 24, 1006, 1024, 7, 24, 18, 60,
            [
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Showers", "🚿"),
                new("Humid", "💧"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Seoul", 37.5665, 126.9780, -8, 31, 40, 78, 8, 26, 1004, 1024, 8, 26, 18, 65,
            [
                new("Cold", "🥶"),
                new("Sunny", "☀️"),
                new("Cloudy", "☁️"),
                new("Windy", "💨"),
                new("Showers", "🚿")
            ]),
            new("Singapore", 1.3521, 103.8198, 25, 33, 74, 96, 5, 14, 1006, 1014, 5, 16, 30, 80,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Rainy", "🌧️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Bangkok", 13.7563, 100.5018, 25, 36, 60, 90, 6, 18, 1004, 1014, 5, 18, 28, 85,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Sunny", "☀️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Jakarta", -6.2088, 106.8456, 24, 33, 72, 95, 5, 14, 1005, 1014, 5, 16, 32, 88,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Rainy", "🌧️"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Manila", 14.5995, 120.9842, 24, 34, 70, 94, 6, 18, 1004, 1014, 6, 18, 28, 85,
            [
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Thunderstorms", "⛈️"),
                new("Showers", "🚿"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Delhi", 28.6139, 77.2090, 7, 42, 25, 70, 7, 24, 1003, 1018, 6, 20, 35, 110,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Cloudy", "☁️"),
                new("Thunderstorms", "⛈️")
            ]),
            new("Hong Kong", 22.3193, 114.1694, 16, 33, 65, 92, 6, 20, 1005, 1018, 7, 22, 24, 75,
            [
                new("Humid", "💧"),
                new("Sunny", "☀️"),
                new("Showers", "🚿"),
                new("Thunderstorms", "⛈️"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Doha", 25.2854, 51.5310, 16, 41, 35, 75, 8, 28, 1004, 1018, 7, 22, 28, 92,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃")
            ]),
            new("Kuwait City", 29.3759, 47.9774, 10, 44, 20, 55, 8, 30, 1002, 1018, 8, 24, 30, 100,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Dry", "🏜️"),
                new("Windy", "💨"),
                new("Clear", "🌤️")
            ]),
            new("Muscat", 23.5880, 58.3829, 20, 39, 40, 78, 8, 26, 1004, 1017, 6, 20, 24, 82,
            [
                new("Sunny", "☀️"),
                new("Hot", "🔥"),
                new("Humid", "💧"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃")
            ]),
            new("Amman", 31.9454, 35.9284, 4, 33, 30, 65, 8, 28, 1008, 1023, 7, 22, 14, 52,
            [
                new("Sunny", "☀️"),
                new("Clear", "🌤️"),
                new("Mild", "🙂"),
                new("Breezy", "🍃"),
                new("Partly Cloudy", "⛅")
            ]),
            new("Jeddah", 21.4858, 39.1925, 23, 39, 45, 80, 8, 28, 1004, 1015, 6, 20, 28, 95,
            [
                new("Hot", "🔥"),
                new("Sunny", "☀️"),
                new("Humid", "💧"),
                new("Clear", "🌤️"),
                new("Breezy", "🍃")
            ]),
        ];

        public static DateTime AlignToForecastBoundary(DateTime utcDateTime)
        {
            var ticks = utcDateTime.Ticks - (utcDateTime.Ticks % ForecastInterval.Ticks);
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static WeatherForecast[] BuildForecasts(DateTime startDateUtc,
                                                       int periods,
                                                       params string[] cities)
        {
            // var previousDateUtc = startDateUtc.AddDays(-1);
            return [.. Cities.Where(c => cities == null || cities.Length == 0 || cities.Contains(c.City))
                             .SelectMany(city => Enumerable.Range(0, periods)
                             .Select(index => CreateForecast(city, startDateUtc.AddTicks(ForecastInterval.Ticks * index))))];
        }

        private static double CalculateDewPointC(int temperatureC, int humidityPercent) =>
            Math.Round(temperatureC - ((100d - humidityPercent) / 5d), 1);

        private static int CalculateFeelsLikeTemperatureC(int temperatureC, int humidityPercent, double windSpeedKph)
        {
            if (temperatureC <= 10 && windSpeedKph > 4.8)
            {
                var windFactor = Math.Pow(windSpeedKph, 0.16);
                var windChill = 13.12 + (0.6215 * temperatureC) - (11.37 * windFactor) + (0.3965 * temperatureC * windFactor);
                return (int)Math.Round(windChill);
            }

            if (temperatureC >= 27 && humidityPercent >= 40)
            {
                var tempF = (temperatureC * 9d / 5d) + 32d;
                var hiF =
                    -42.379 +
                    (2.04901523 * tempF) +
                    (10.14333127 * humidityPercent) -
                    (0.22475541 * tempF * humidityPercent) -
                    (0.00683783 * tempF * tempF) -
                    (0.05481717 * humidityPercent * humidityPercent) +
                    (0.00122874 * tempF * tempF * humidityPercent) +
                    (0.00085282 * tempF * humidityPercent * humidityPercent) -
                    (0.00000199 * tempF * tempF * humidityPercent * humidityPercent);

                return (int)Math.Round((hiF - 32d) * 5d / 9d);
            }

            return temperatureC;
        }

        private static WeatherForecast CreateForecast(CityWeatherProfile city, DateTime dateUtc)
        {
            var summary = GetDeterministicSummary(city, dateUtc);
            var temperatureC = GetDeterministicTemperature(city, dateUtc);
            var (minimumTemperatureC, maximumTemperatureC) = GetDeterministicTemperatureRange(city, dateUtc, temperatureC);
            var humidityPercent = WeatherForecastEngine.GetDeterministicHumidity(city, dateUtc, summary.Text, temperatureC);
            var pressureHpa = GetDeterministicPressure(city, dateUtc);
            var visibilityKm = WeatherForecastEngine.GetDeterministicVisibility(city, dateUtc, humidityPercent, summary.Text);
            var airQualityIndex = GetDeterministicAirQualityIndex(city, dateUtc);
            var daylightHours = GetDeterministicDaylightHours(city, dateUtc);
            var uvIndex = WeatherForecastEngine.GetDeterministicUvIndex(city, dateUtc, daylightHours, summary.Text);
            var rainProbabilityPercent = WeatherForecastEngine.GetDeterministicRainProbability(city, dateUtc, humidityPercent, summary.Text);
            var wind = GetDeterministicWind(city, dateUtc);
            var feelsLikeTemperatureC = WeatherForecastEngine.CalculateFeelsLikeTemperatureC(temperatureC, humidityPercent, wind.SpeedKph);
            var dewPointC = CalculateDewPointC(temperatureC, humidityPercent);

            var isDaytime = GetIsDaytime(city, dateUtc, daylightHours);
            var cloudCoverPercent = WeatherForecastEngine.GetDeterministicCloudCoverPercent(summary.Text, humidityPercent, rainProbabilityPercent, isDaytime);
            var precipitationType = WeatherForecastEngine.GetDeterministicPrecipitationType(summary.Text, temperatureC);
            var precipitationMm = WeatherForecastEngine.GetDeterministicPrecipitationMm(summary.Text, rainProbabilityPercent, precipitationType, dateUtc, city.City);

            return new WeatherForecast
            {
                City = city.City,
                Date = dateUtc,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                TemperatureC = temperatureC,
                FeelsLikeTemperatureC = feelsLikeTemperatureC,
                MinimumTemperatureC = minimumTemperatureC,
                MaximumTemperatureC = maximumTemperatureC,
                HumidityPercent = humidityPercent,
                DewPointC = dewPointC,
                PressureHpa = pressureHpa,
                VisibilityKm = visibilityKm,
                AirQualityIndex = airQualityIndex,
                UVIndex = uvIndex,
                RainProbabilityPercent = rainProbabilityPercent,
                CloudCoverPercent = cloudCoverPercent,
                PrecipitationMm = precipitationMm,
                PrecipitationType = precipitationType,
                IsDaytime = isDaytime,
                WindSpeedKph = wind.SpeedKph,
                WindGustKph = wind.GustKph,
                WindDirectionDegrees = wind.DirectionDegrees,
                WindDirection = wind.DirectionText,
                DaylightHours = daylightHours,
                ActivityRecommendation = WeatherForecastEngine.GetActivityRecommendation(
                    summary.Text,
                    rainProbabilityPercent,
                    airQualityIndex,
                    uvIndex,
                    feelsLikeTemperatureC,
                    wind.SpeedKph),
                Summary = summary.Text,
                SummaryIcon = summary.Icon
            };
        }

        private static Random CreateRandom(string city, DateTime dateUtc, int salt) =>
            new(GetDeterministicSeed(city, dateUtc, salt));

        private static string GetActivityRecommendation(
            string summary,
            int rainProbabilityPercent,
            int airQualityIndex,
            int uvIndex,
            int feelsLikeTemperatureC,
            double windSpeedKph)
        {
            if (airQualityIndex > 100)
            {
                return "Limit outdoor activity";
            }

            if (rainProbabilityPercent >= 70)
            {
                return "Carry an umbrella";
            }

            if (uvIndex >= 8)
            {
                return "Use sunscreen and seek shade";
            }

            if (windSpeedKph >= 30)
            {
                return "Secure loose outdoor items";
            }

            if (feelsLikeTemperatureC >= 30)
            {
                return "Stay hydrated";
            }

            if (feelsLikeTemperatureC <= 5)
            {
                return "Dress warmly";
            }

            return summary switch
            {
                "Sunny" or "Partly Cloudy" => "Good for outdoor activities",
                "Cloudy" or "Mild" => "Great for a walk",
                "Windy" or "Breezy" => "Good for light outdoor exercise",
                _ => "Conditions are moderate"
            };
        }

        private static string GetCardinalDirection(int degrees)
        {
            string[] directions = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];
            var index = (int)Math.Round(degrees / 45d, MidpointRounding.AwayFromZero) % directions.Length;

            return directions[index];
        }

        private static int GetDeterministicAirQualityIndex(CityWeatherProfile city, DateTime dateUtc) => WeatherForecastEngine.CreateRandom(city.City, dateUtc, AirQualitySeedSalt)
                .Next(city.MinAirQualityIndex, city.MaxAirQualityIndex + 1);

        private static int GetDeterministicCloudCoverPercent(
            string summary,
            int humidityPercent,
            int rainProbabilityPercent,
            bool isDaytime)
        {
            var baseCloudCover = summary switch
            {
                "Clear" => 5,
                "Sunny" => 10,
                "Warm" => 15,
                "Partly Cloudy" => 35,
                "Mild" => 40,
                "Breezy" => 45,
                "Cloudy" => 70,
                "Overcast" => 90,
                "Light Rain" => 85,
                "Showers" => 75,
                "Rainy" => 95,
                "Thunderstorms" => 100,
                "Snow Showers" => 95,
                _ => 50
            };

            var adjusted = baseCloudCover + ((humidityPercent - 60) / 3) + (rainProbabilityPercent / 10) + (isDaytime ? 0 : 5);
            return Math.Clamp(adjusted, 0, 100);
        }

        private static double GetDeterministicDaylightHours(CityWeatherProfile city, DateTime dateUtc)
        {
            var dayOfYear = dateUtc.DayOfYear;
            var latitudeFactor = Math.Min(Math.Abs(city.Latitude) / 90d, 1d);
            var amplitude = 1.2 + (latitudeFactor * 4.8);
            var seasonalWave = Math.Sin((2d * Math.PI * (dayOfYear - 80)) / 365.25);

            var daylightHours = 12d + (amplitude * seasonalWave);
            return Math.Round(Math.Clamp(daylightHours, 10d, 18d), 1);
        }

        private static int GetDeterministicHumidity(CityWeatherProfile city, DateTime dateUtc, string summary, int temperatureC)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc, HumiditySeedSalt);
            var localHour = GetLocalTime(city, dateUtc).TimeOfDay.TotalHours;

            var averageHumidity = (city.MinHumidityPercent + city.MaxHumidityPercent) / 2d;
            var averageTemperature = ((city.MinTempC + city.MaxTempC) / 2d) + WeatherForecastEngine.GetSeasonAdjustment(city.City, dateUtc.Month);

            var humidity = averageHumidity - ((temperatureC - averageTemperature) * 2d);
            humidity += localHour < 7d || localHour >= 20d ? 6d : -3d;

            humidity += summary switch
            {
                "Humid" => 6,
                "Thunderstorms" or "Rainy" or "Light Rain" or "Showers" => 10,
                "Sunny" or "Hot" => -8,
                "Windy" or "Breezy" => -4,
                _ => 0
            };

            humidity += random.Next(-5, 6);

            return Math.Clamp((int)Math.Round(humidity), 35, 100);
        }

        private static int GetDeterministicHumidity(CityWeatherProfile city, DateTime dateUtc, string summary)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc, HumiditySeedSalt);
            var humidity = random.Next(city.MinHumidityPercent, city.MaxHumidityPercent + 1);

            humidity += summary switch
            {
                "Humid" => 5,
                "Thunderstorms" or "Rainy" or "Light Rain" or "Showers" => 7,
                "Sunny" => -8,
                "Windy" or "Breezy" => -4,
                _ => 0
            };

            return Math.Clamp(humidity, 35, 100);
        }

        private static double GetDeterministicPrecipitationMm(
            string summary,
            int rainProbabilityPercent,
            string precipitationType,
            DateTime dateUtc,
            string city)
        {
            if (precipitationType == "None")
            {
                return 0;
            }

            var random = CreateRandom(city, dateUtc, 200);
            var baseAmount = summary switch
            {
                "Thunderstorms" => 6.0 + (random.NextDouble() * 12.0),
                "Rainy" => 3.0 + (random.NextDouble() * 8.0),
                "Showers" => 1.0 + (random.NextDouble() * 5.0),
                "Light Rain" => 0.2 + (random.NextDouble() * 2.5),
                "Snow Showers" => 0.5 + (random.NextDouble() * 3.0),
                _ => 0.1 + (random.NextDouble() * 1.5)
            };

            var scaled = baseAmount * (rainProbabilityPercent / 100d);
            return Math.Round(scaled, 1);
        }

        private static string GetDeterministicPrecipitationType(string summary, int temperatureC) =>
            summary switch
            {
                "Thunderstorms" => "Thunderstorm",
                "Rainy" or "Light Rain" or "Showers" => temperatureC <= 1 ? "Snow" : "Rain",
                "Snow Showers" => "Snow",
                _ => "None"
            };

        private static int GetDeterministicPressure(CityWeatherProfile city, DateTime dateUtc)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, GetWeatherSystemStart(dateUtc), PressureSeedSalt);
            var localHour = GetLocalTime(city, dateUtc).TimeOfDay.TotalHours;

            var basePressure = city.MinPressureHpa + (random.NextDouble() * (city.MaxPressureHpa - city.MinPressureHpa));
            var hourlyWave = Math.Sin((2d * Math.PI * localHour) / 24d) * 1.6;

            return (int)Math.Round(Math.Clamp(basePressure + hourlyWave, city.MinPressureHpa, city.MaxPressureHpa));
        }

        private static int GetDeterministicRainProbability(
            CityWeatherProfile city,
            DateTime dateUtc,
            int humidityPercent,
            string summary)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc, RainProbabilitySeedSalt);
            var baseProbability = random.Next(5, 45) + ((humidityPercent - 50) / 2);

            baseProbability += summary switch
            {
                "Thunderstorms" => 45,
                "Rainy" => 35,
                "Light Rain" or "Showers" => 30,
                "Cloudy" or "Overcast" => 15,
                "Partly Cloudy" => 8,
                "Sunny" => -15,
                _ => 0
            };

            return Math.Clamp(baseProbability, 0, 100);
        }

        private static int GetDeterministicSeed(string city, DateTime dateUtc, int salt)
        {
            unchecked
            {
                var hash = 17;

                foreach (var character in city)
                {
                    hash = (hash * 31) + character;
                }

                hash = (hash * 31) + dateUtc.Year;
                hash = (hash * 31) + dateUtc.Month;
                hash = (hash * 31) + dateUtc.Day;
                hash = (hash * 31) + dateUtc.Hour;
                hash = (hash * 31) + salt;

                return hash;
            }
        }

        private static WeatherSummary GetDeterministicSummary(CityWeatherProfile city, DateTime dateUtc)
        {
            var systemStart = GetWeatherSystemStart(dateUtc);
            var random = WeatherForecastEngine.CreateRandom(city.City, systemStart, SummarySeedSalt);
            var localHour = GetLocalTime(city, dateUtc).TimeOfDay.TotalHours;
            var moistureIndex = random.NextDouble();

            if (moistureIndex > 0.88 && localHour >= 13d && localHour <= 20d)
            {
                return GetPreferredSummary(city, "Thunderstorms", "Rainy", "Showers", "Cloudy");
            }

            if (moistureIndex > 0.72)
            {
                return GetPreferredSummary(city, "Rainy", "Light Rain", "Showers", "Overcast", "Cloudy");
            }

            if (localHour < 6d || localHour >= 20d)
            {
                return GetPreferredSummary(city, "Clear", "Overcast", "Cloudy", "Mild");
            }

            if (moistureIndex < 0.24)
            {
                return GetPreferredSummary(city, "Sunny", "Warm", "Hot", "Clear");
            }

            if (moistureIndex < 0.48)
            {
                return GetPreferredSummary(city, "Partly Cloudy", "Mild", "Breezy", "Sunny");
            }

            return GetPreferredSummary(city, "Cloudy", "Overcast", "Cool", "Partly Cloudy");
        }

        private static int GetDeterministicTemperature(CityWeatherProfile city, DateTime dateUtc)
        {
            var seasonalAdjustment = WeatherForecastEngine.GetSeasonAdjustment(city.City, dateUtc.Month);
            var localTime = GetLocalTime(city, dateUtc);
            var localHour = localTime.TimeOfDay.TotalHours;

            var midpoint = ((city.MinTempC + city.MaxTempC) / 2d) + seasonalAdjustment;
            var amplitude = Math.Max(3d, ((city.MaxTempC - city.MinTempC) / 2d) - 1d);

            var diurnalFactor = Math.Cos((2d * Math.PI * (localHour - 15d)) / 24d);
            var systemNoise = (WeatherForecastEngine.CreateRandom(city.City, GetWeatherSystemStart(dateUtc), TemperatureSeedSalt).NextDouble() * 4d) - 2d;
            var hourlyNoise = (WeatherForecastEngine.CreateRandom(city.City, dateUtc, TemperatureSeedSalt + 100).NextDouble() * 1.4d) - 0.7d;

            var temperature = midpoint + (amplitude * diurnalFactor) + systemNoise + hourlyNoise;

            return Math.Clamp(
                (int)Math.Round(temperature),
                city.MinTempC + seasonalAdjustment - 4,
                city.MaxTempC + seasonalAdjustment + 4);
        }

        private static (int MinimumTemperatureC, int MaximumTemperatureC) GetDeterministicTemperatureRange(
            CityWeatherProfile city,
            DateTime dateUtc,
            int currentTemperatureC)
        {
            var seasonalAdjustment = WeatherForecastEngine.GetSeasonAdjustment(city.City, dateUtc.Month);
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc.Date.AddHours(12), TemperatureRangeSeedSalt);

            var baseMin = city.MinTempC + seasonalAdjustment + random.Next(-2, 3);
            var baseMax = city.MaxTempC + seasonalAdjustment + random.Next(-2, 3);

            var minimumTemperatureC = Math.Min(baseMin, currentTemperatureC - 1);
            var maximumTemperatureC = Math.Max(baseMax, currentTemperatureC + 1);

            return (minimumTemperatureC, maximumTemperatureC);
        }

        private static int GetDeterministicUvIndex(
            CityWeatherProfile city,
            DateTime dateUtc,
            double daylightHours,
            string summary)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc, UvSeedSalt);
            var localTime = dateUtc + TimeSpan.FromHours(city.Longitude / 15d);
            var localHour = localTime.TimeOfDay.TotalHours;

            if (localHour < 6d || localHour > 18d)
            {
                return 0;
            }

            var solarFactor = Math.Sin(Math.PI * ((localHour - 6d) / 12d));
            var daylightFactor = daylightHours / 12d;
            var cloudModifier = summary switch
            {
                "Thunderstorms" or "Rainy" or "Light Rain" or "Showers" => 0.45,
                "Cloudy" or "Overcast" => 0.65,
                "Partly Cloudy" => 0.8,
                _ => 1.0
            };

            var uv = (7d * daylightFactor * solarFactor * cloudModifier) + (random.NextDouble() * 2d);
            return Math.Clamp((int)Math.Round(uv), 0, 11);
        }

        private static double GetDeterministicVisibility(
            CityWeatherProfile city,
            DateTime dateUtc,
            int humidityPercent,
            string summary)
        {
            var random = WeatherForecastEngine.CreateRandom(city.City, dateUtc, VisibilitySeedSalt);
            var visibility = city.MinVisibilityKm + (random.NextDouble() * (city.MaxVisibilityKm - city.MinVisibilityKm));

            visibility -= summary switch
            {
                "Thunderstorms" => 4.0,
                "Rainy" or "Light Rain" or "Showers" => 2.5,
                "Cloudy" or "Overcast" => 1.0,
                _ => 0.0
            };

            visibility -= Math.Max(0, humidityPercent - 75) / 25.0;
            return Math.Round(Math.Clamp(visibility, 2.0, city.MaxVisibilityKm), 1);
        }

        private static WindData GetDeterministicWind(CityWeatherProfile city, DateTime dateUtc)
        {
            var systemStart = GetWeatherSystemStart(dateUtc);
            var random = WeatherForecastEngine.CreateRandom(city.City, systemStart, WindSeedSalt);
            var hourlyRandom = WeatherForecastEngine.CreateRandom(city.City, dateUtc, WindSeedSalt + 50);
            var localHour = GetLocalTime(city, dateUtc).TimeOfDay.TotalHours;

            var baseSpeed = city.MinWindSpeedKph + (random.NextDouble() * (city.MaxWindSpeedKph - city.MinWindSpeedKph));
            var daytimeBoost = localHour >= 11d && localHour <= 17d ? 2.5 : localHour < 6d ? -1.5 : 0d;
            var speedKph = Math.Clamp(baseSpeed + daytimeBoost + ((hourlyRandom.NextDouble() * 2d) - 1d), city.MinWindSpeedKph, city.MaxWindSpeedKph + 4d);
            var gustKph = speedKph + 4d + (random.NextDouble() * 10d);

            var directionDegrees = (random.Next(0, 360) + (int)Math.Round((localHour - 12d) * 2d)) % 360;
            if (directionDegrees < 0)
            {
                directionDegrees += 360;
            }

            return new WindData(
                Math.Round(speedKph, 1),
                Math.Round(gustKph, 1),
                directionDegrees,
                GetCardinalDirection(directionDegrees));
        }

        public static DateTime GetForecastWindowStart(DateTime currentUtc) =>
            new(currentUtc.Year, currentUtc.Month, currentUtc.Day, 0, 0, 0, DateTimeKind.Utc);

        private static bool GetIsDaytime(CityWeatherProfile city, DateTime dateUtc, double daylightHours)
        {
            var localTime = dateUtc + TimeSpan.FromHours(city.Longitude / 15d);
            var sunriseHour = 12d - (daylightHours / 2d);
            var sunsetHour = 12d + (daylightHours / 2d);
            var localHour = localTime.TimeOfDay.TotalHours;

            return localHour >= sunriseHour && localHour < sunsetHour;
        }

        private static DateTime GetLocalTime(CityWeatherProfile city, DateTime utcDateTime) =>
            utcDateTime + TimeSpan.FromHours(city.Longitude / 15d);

        private static WeatherSummary GetPreferredSummary(CityWeatherProfile city, params string[] preferred)
        {
            foreach (var text in preferred)
            {
                var summary = city.Summaries.FirstOrDefault(s => s.Text == text);
                if (summary is not null)
                {
                    return summary;
                }
            }

            return city.Summaries[0];
        }

        private static int GetSeasonAdjustment(string city, int month) =>
            city switch
            {
                "London" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 4 : 0,
                "Paris" => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 5 : 0,
                "New York" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 7 : 0,
                "Toronto" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 6 : 0,
                "Montreal" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 6 : 0,
                "Anchorage" => month is 12 or 1 or 2 ? -14 : month is >= 6 and <= 8 ? 5 : 0,
                "Fairbanks" => month is 12 or 1 or 2 ? -18 : month is >= 6 and <= 8 ? 8 : 0,
                "Yellowknife" => month is 12 or 1 or 2 ? -18 : month is >= 6 and <= 8 ? 8 : 0,
                "Iqaluit" or "Nuuk" => month is 12 or 1 or 2 ? -16 : month is >= 6 and <= 8 ? 4 : 0,
                "Oslo" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 4 : 0,
                "Reykjavik" => month is 12 or 1 or 2 ? -6 : month is >= 6 and <= 8 ? 2 : 0,
                "Cardiff" or "Edinburgh" or "Dublin" or "Belfast" => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 3 : 0,
                "Helsinki" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 5 : 0,
                "Tromso" or "Murmansk" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 4 : 0,
                "Moscow" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 5 : 0,
                "Novosibirsk" => month is 12 or 1 or 2 ? -18 : month is >= 6 and <= 8 ? 8 : 0,
                "Yakutsk" => month is 12 or 1 or 2 ? -22 : month is >= 6 and <= 8 ? 8 : 0,
                "Vladivostok" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 6 : 0,
                "Beijing" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 8 : 0,
                "Madrid" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 8 : 0,
                "Los Angeles" => month is 12 or 1 or 2 ? 1 : month is >= 6 and <= 8 ? 4 : 0,
                "Dallas" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 8 : 0,
                "Miami" => month is 12 or 1 or 2 ? 2 : month is >= 6 and <= 8 ? 3 : 0,
                "Mexico City" => month is 12 or 1 or 2 ? -1 : month is >= 4 and <= 6 ? 3 : 0,
                "Cairo" or "Dubai" or "Riyadh" or "Tehran" or "Jerusalem" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 7 : 0,
                "Mumbai" => month is >= 6 and <= 9 ? -1 : month is >= 3 and <= 5 ? 2 : 0,
                "Bogota" => 0,
                "Caracas" or "Kingston" or "Lagos" or "Kuala Lumpur" => month is >= 6 and <= 9 ? -1 : 1,
                "Manaus" => month is >= 6 and <= 9 ? -1 : month is 12 or 1 or 2 ? 1 : 0,
                "Nairobi" or "Addis Ababa" => month is >= 6 and <= 9 ? -1 : month is >= 1 and <= 3 ? 1 : 0,
                "Rio de Janeiro" or "Sydney" or "Cape Town" or "Buenos Aires" => month is >= 6 and <= 8 ? -4 : month is 12 or 1 or 2 ? 5 : 0,
                "Lima" => month is >= 6 and <= 8 ? -2 : month is 12 or 1 or 2 ? 3 : 0,
                "Santiago" => month is >= 6 and <= 8 ? -5 : month is 12 or 1 or 2 ? 6 : 0,
                "Johannesburg" or "Antananarivo" => month is >= 6 and <= 8 ? -4 : month is 12 or 1 or 2 ? 4 : 0,
                "Port Louis" or "Suva" => month is >= 6 and <= 8 ? -2 : month is 12 or 1 or 2 ? 3 : 0,
                "Honolulu" => month is 12 or 1 or 2 ? -1 : month is >= 7 and <= 9 ? 2 : 0,
                "Abuja" => month is >= 6 and <= 9 ? -2 : month is >= 3 and <= 5 ? 2 : 0,
                "Maiduguri" => month is 12 or 1 or 2 ? -1 : month is >= 3 and <= 6 ? 4 : month is >= 7 and <= 9 ? -2 : 1,
                "Chicago" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 6 : 0,
                "Seattle" or "Vancouver" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 3 : 0,
                "San Francisco" => month is 12 or 1 or 2 ? 0 : month is >= 8 and <= 10 ? 2 : 0,
                "Denver" or "Calgary" => month is 12 or 1 or 2 ? -10 : month is >= 6 and <= 8 ? 6 : 0,
                "Atlanta" or "Washington, D.C." => month is 12 or 1 or 2 ? -4 : month is >= 6 and <= 8 ? 6 : 0,
                "Phoenix" => month is 12 or 1 or 2 ? 2 : month is >= 6 and <= 8 ? 8 : 0,
                "Ottawa" => month is 12 or 1 or 2 ? -12 : month is >= 6 and <= 8 ? 6 : 0,
                "Tokyo" or "Seoul" => month is 12 or 1 or 2 ? -8 : month is >= 6 and <= 8 ? 7 : 0,
                "Singapore" or "Jakarta" => month is >= 6 and <= 9 ? -1 : 1,
                "Bangkok" or "Manila" => month is >= 3 and <= 5 ? 2 : month is >= 6 and <= 10 ? -1 : 0,
                "Delhi" => month is 12 or 1 or 2 ? -5 : month is >= 4 and <= 6 ? 8 : month is >= 7 and <= 9 ? -2 : 0,
                "Hong Kong" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 9 ? 3 : 0,
                "Doha" or "Kuwait City" => month is 12 or 1 or 2 ? -2 : month is >= 6 and <= 8 ? 8 : 0,
                "Muscat" or "Jeddah" => month is 12 or 1 or 2 ? 0 : month is >= 6 and <= 8 ? 6 : 0,
                "Amman" => month is 12 or 1 or 2 ? -3 : month is >= 6 and <= 8 ? 6 : 0,
                _ => 0
            };

        private static DateTime GetWeatherSystemStart(DateTime utcDateTime)
        {
            var ticks = utcDateTime.Ticks - (utcDateTime.Ticks % WeatherSystemInterval.Ticks);
            return new DateTime(ticks, DateTimeKind.Utc);
        }


        public static string GetAqiLabel(int aqi) =>
        aqi switch
        {
            <= 50 => "Good",
            <= 100 => "Moderate",
            <= 150 => "Unhealthy for sensitive groups",
            <= 200 => "Unhealthy",
            <= 300 => "Very unhealthy",
            _ => "Hazardous"
        };

        public static string GetUvLabel(int uvIndex) =>
            uvIndex switch
            {
                <= 2 => "Low",
                <= 5 => "Moderate",
                <= 7 => "High",
                <= 10 => "Very high",
                _ => "Extreme"
            };
    }
}